using BSServer.BSSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BSServer
{
    public partial class FrmMain : Form
    {
        public static readonly ConcurrentQueue<string> LogList = new ConcurrentQueue<string>();

        public static BSCore Core = new BSCore();

        public FrmMain()
        {
            InitializeComponent();
        }

        public static void Enqueue(Exception ex)
        {
            LogList.Enqueue(String.Format("[{0}]: {1} - {2}" + Environment.NewLine, DateTime.Now, ex.TargetSite, ex));
            File.AppendAllText(Settings.LOG_PATH + "Event Log (" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ").txt",
                                               String.Format("[{0}]: {1} - {2}" + Environment.NewLine, DateTime.Now, ex.TargetSite, ex));
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }

        public static void Enqueue(string msg)
        {
            LogList.Enqueue(String.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, msg));
            File.AppendAllText(Settings.LOG_PATH + "Log (" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ").txt",
                                           String.Format("[{0}]: {1}" + Environment.NewLine, DateTime.Now, msg));
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                while (!LogList.IsEmpty)
                {
                    string message;
                    if (!LogList.TryDequeue(out message)) continue;

                    txtLog.AppendText(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString()); // DEBUG
            }
        }
    }
}
