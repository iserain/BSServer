using BSServer.BSSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BSServer.BSNetwork
{
    
    public class BSNetworkModule
    {
        private BSCore Core { get { return FrmMain.Core; } }
        private TcpListener mListener;
        private int SessionCount;

        public BSNetworkModule()
        {
            mListener = new TcpListener(IPAddress.Parse(Settings.IPAddress), Settings.Port);
            mListener.Start();
            mListener.BeginAcceptTcpClient(BeginConnection, null);
        }
        
        private void BeginConnection(IAsyncResult result)
        {
            if (!Core.isRunning || mListener.Server.IsBound) return;

            try
            {
                TcpClient _tcpClient = mListener.EndAcceptTcpClient(result);
                lock (Core.ConnectionList)
                    Core.ConnectionList.Add(new BSConnection(++SessionCount, _tcpClient));
            }
            catch (Exception ex)
            {
                FrmMain.Enqueue(ex);
            }
            finally
            {
                if (Core.isRunning && mListener.Server.IsBound)
                    mListener.BeginAcceptTcpClient(BeginConnection, null);
            }
        }
    }
}
