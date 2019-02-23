using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSServer
{
    public static class Settings
    {
        public static bool TestMode = false;


        public const string LOG_PATH = @".\Logs\",
            BACKUP_PATH = @".\Backup\",
            ACCOUNT_DATABASE_PATH = @".\Account.BSDB";

        public static bool Multithreaded = true;
        public static int ThreadLimit = 2;

        public static string IPAddress = "127.0.0.1";
        public static int Port = 7000;
        public static long ConnectionTimeout = 30000;
        
    }
}
