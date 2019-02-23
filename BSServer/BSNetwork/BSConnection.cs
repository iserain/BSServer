using BSServer.BSSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using R = BSServer.BSReceivePacketCatalog;
using S = BSServer.BSSendPacketCatalog;

namespace BSServer.BSNetwork
{
    public class BSConnection
    {
        private BSCore Core { get { return FrmMain.Core; } }

        public readonly int SessionID;
        public readonly string IPAddress;

        public bool Connected;
        private bool _disconnecting;
        public bool Disconnecting
        {
            get { return _disconnecting; }
            set
            {
                if (_disconnecting == value) return;
                _disconnecting = value;
                TimeoutTime = Core.Time + 500; // 0.5sec 
            }
        }

        private TcpClient mClient;
        private ConcurrentQueue<BSPacket> mReceiveList;
        private Queue<BSPacket> mSendList, mRetryList;
        private byte[] mRawData = new byte[0];

        public readonly long ConnectedTime;
        public long DisconnectedTime;
        public long TimeoutTime;

        public BSConnection(int sessionID, TcpClient client) 
        {
            SessionID = sessionID;
            IPAddress = client.Client.RemoteEndPoint.ToString().Split(':')[0];

            bool _detected = false;
            foreach (BSConnection _conn in Core.ConnectionList)
            {
                if (_conn.IPAddress == IPAddress)
                {
                    FrmMain.Enqueue("Concurrent Connection Detected. (" + IPAddress + ")");
                    _detected = true;
                    break;
                }
            }

            if (_detected) return;

            FrmMain.Enqueue(IPAddress + ", Connected.");

            mClient = client;
            mClient.NoDelay = true;

            ConnectedTime = Core.Time;
            TimeoutTime = ConnectedTime + Settings.ConnectionTimeout;

            mReceiveList = new ConcurrentQueue<BSPacket>();
            mSendList = new Queue<BSPacket>();
            mRetryList = new Queue<BSPacket>();

            Connected = true;
            BeginReceive();
        }

        private void BeginReceive()
        {
            if (!Connected) return;

            byte[] rawBytes = new byte[8 * 1024];
            try
            {
                mClient.Client.BeginReceive(rawBytes, 0, rawBytes.Length, SocketFlags.None, ReceiveData, rawBytes);
            }
            catch
            {
                Disconnecting = true;   
            }
        }

        private void ReceiveData(IAsyncResult result)
        {
            if (!Connected) return;

            int dataRead;
            try
            {
                dataRead = mClient.Client.EndReceive(result);
            }
            catch
            {
                Disconnecting = true;
                return;
            }

            byte[] rawBytes = result.AsyncState as byte[];
            byte[] temp = mRawData;
            mRawData = new byte[dataRead + temp.Length];
            Buffer.BlockCopy(temp, 0, mRawData, 0, temp.Length);
            Buffer.BlockCopy(rawBytes, 0, mRawData, temp.Length, dataRead);

            BSPacket p;
            while ((p = BSPacket.ReceivePacket(mRawData, out mRawData)) != null)
                mReceiveList.Enqueue(p);

            BeginReceive();
        }

        private void BeginSend(List<byte> data)
        {
            if (!Connected || data.Count == 0) return;
            
            try
            {
                mClient.Client.BeginSend(data.ToArray(), 0, data.Count, SocketFlags.None, SendData, Disconnecting);
            }
            catch
            {
                Disconnecting = true;
            }
        }

        private void SendData(IAsyncResult result)
        {
            try
            {
                mClient.Client.EndSend(result);
            }
            catch
            {
                // Disconnecting = true;
            }
        }

        public void Enqueue(BSPacket p)
        {
            if (mSendList != null && p != null)
                mSendList.Enqueue(p);
        }

        public void Process()
        {
            if (mClient == null || !mClient.Connected)
            {
                // Socket Disconnected
                return;
            }

            while (!mReceiveList.IsEmpty && !Disconnecting)
            {
                BSPacket p;
                if (!mReceiveList.TryDequeue(out p)) continue;
                TimeoutTime = Core.Time + Settings.ConnectionTimeout;
                ProcessPacket(p);
            }

            while (mRetryList.Count > 0)
                mReceiveList.Enqueue(mRetryList.Dequeue());

            if (Core.Time > TimeoutTime)
            {
                // Timeout Disconnected
                return;
            }

            if (mSendList == null || mSendList.Count <= 0) return;

            List<byte> data = new List<byte>();
            while (mSendList.Count > 0)
            {
                BSPacket p = mSendList.Dequeue();
                data.AddRange(p.GetPacketBytes());
            }

            BeginSend(data);
        }

        private void ProcessPacket(BSPacket p)
        {
            if (p == null || Disconnecting) return;

            switch (p.Index)
            {
                case (short)R.ReceivePacketIds.KeepAlive:
                    KeepAlive((R.KeepAlive)p);
                    break;
                case (short)R.ReceivePacketIds.ClientVersion:
                    ClientVersion((R.ClientVersion)p);
                    break;
            }
        }

        public void Disconnect(byte reason)
        {
            if (!Connected) return;

            Connected = false;
            DisconnectedTime = Core.Time;

            lock (Core.ConnectionList)
                Core.ConnectionList.Remove(this);

            mReceiveList = null;
            mSendList = null;
            mRetryList = null;
            mRawData = null;

            if (mClient != null) mClient.Client.Dispose();
            mClient = null;
        }

        public void SendDisconnect(byte reason)
        {
            if (!Connected)
            {
                Disconnecting = true;
                SoftDisconnect(reason);
                return;
            }

            Disconnecting = true;
            List<byte> data = new List<byte>();
            data.AddRange(new S.Disconnect { Reason = reason }.GetPacketBytes());

            BeginSend(data);
            SoftDisconnect(reason);
        }

        private void SoftDisconnect(byte reason)
        {
            DisconnectedTime = Core.Time;
            

        }


        private void KeepAlive(R.KeepAlive p)
        {
            Enqueue(new S.AliveRetrun() { Time = Core.Time });
        }
        private void ClientVersion(R.ClientVersion p)
        {
            
        }
    }
}
