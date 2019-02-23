using BSServer.BSNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSServer.BSReceivePacketCatalog
{
    public enum ReceivePacketIds
    {
        KeepAlive,
        ClientVersion,
    }

    public sealed class KeepAlive : BSPacket
    {
        public override short Index
        {
            get { return (short)ReceivePacketIds.KeepAlive; }
        }
        public long Time;

        protected override void ReadPacket(BinaryReader reader)
        {
            Time = reader.ReadInt64();
        }

        protected override void WritePacket(BinaryWriter writer)
        {
        }
    }

    public sealed class ClientVersion : BSPacket
    {
        public override short Index
        {
            get { return (short)ReceivePacketIds.KeepAlive; }
        }
        public int Version;
        public long Hash;

        protected override void ReadPacket(BinaryReader reader)
        {
            Version = reader.ReadInt32();
            Hash = reader.ReadInt64();
        }

        protected override void WritePacket(BinaryWriter writer)
        {
        }
    }
}
