using BSServer.BSNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSServer.BSSendPacketCatalog
{
    
    public enum SendPacketIds
    {
        AliveRetrun,
        Disconnect,
    }

    public sealed class AliveRetrun : BSPacket
    {
        public override short Index
        {
            get { return (short)SendPacketIds.AliveRetrun; }
        }
        public long Time;

        protected override void ReadPacket(BinaryReader reader)
        {
        }

        protected override void WritePacket(BinaryWriter writer)
        {
            writer.Write(Time);
        }
    }
    public sealed class Disconnect : BSPacket
    {
        public override short Index
        {
            get { return (short)SendPacketIds.AliveRetrun; }
        }
        public byte Reason;

        protected override void ReadPacket(BinaryReader reader)
        {
        }

        protected override void WritePacket(BinaryWriter writer)
        {
            writer.Write(Reason);
        }
    }
}
