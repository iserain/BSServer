using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSServer.BSSystem
{
    public class BSThreadInfo
    {
        public int Id = 0;
        public long LastRunTime = 0;
        public long StartTime = 0;
        public long EndTime = 0;
        public bool Stop = false; // Stop Flag

        public BSThreadInfo(int id)
        {
            Id = id;
        }
    }
}