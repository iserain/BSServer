using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R = BSServer.BSReceivePacketCatalog;
using S = BSServer.BSSendPacketCatalog;

namespace BSServer.BSNetwork
{
    public abstract class BSPacket
    {
        public abstract short Index { get; }

        protected abstract void ReadPacket(BinaryReader reader);
        protected abstract void WritePacket(BinaryWriter writer);

        public static BSPacket ReceivePacket(byte[] rawBytes, out byte[] extra)
        {
            extra = rawBytes;
            BSPacket p;

            if (rawBytes.Length < 4) return null;

            int length = (rawBytes[1] << 8) + rawBytes[0];

            if (length > rawBytes.Length || length < 2) return null;

            using (MemoryStream stream = new MemoryStream(rawBytes, 2, length - 2))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    short id = reader.ReadInt16();

                    p = GetPacketData(id);
                    if (p == null) return null;

                    p.ReadPacket(reader);
                }
                catch
                {
                    return null;
                }
            }

            extra = new byte[rawBytes.Length - length];
            Buffer.BlockCopy(rawBytes, length, extra, 0, rawBytes.Length - length);

            return p;
        }

        public IEnumerable<byte> GetPacketBytes()
        {
            if (Index < 0) return new byte[0];

            byte[] data;

            using (MemoryStream stream = new MemoryStream())
            {
                stream.SetLength(2);
                stream.Seek(2, SeekOrigin.Begin);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Index);
                    WritePacket(writer);
                    stream.Seek(0, SeekOrigin.Begin);
                    writer.Write((short)stream.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }

            return data;
        }

        private static BSPacket GetPacketData(short index)
        {
            switch (index)
            {
                case (short)R.ReceivePacketIds.KeepAlive:
                    return new R.KeepAlive();
                case (short)R.ReceivePacketIds.ClientVersion:
                    return new R.ClientVersion();

                default:
                    return null;
            }
        }
    }
}
