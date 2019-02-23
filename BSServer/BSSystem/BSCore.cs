using BSServer.BSNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BSServer.BSSystem
{
    public class BSCore
    {
        public const int CoreVersion = 1;
        public int CurrentVersion;

        private DateTime _startTime = DateTime.Now;
        private Stopwatch _stopWatch = Stopwatch.StartNew();
        public long Time { get; private set; }
        public DateTime Now
        {
            get { return _startTime.AddMilliseconds(Time); }
        }

        public BSRandomProvider Random = new BSRandomProvider();

        public bool isRunning { get; private set; }
        private Thread CoreThread;

        // Multithread
        private object _threadLocker = new object();
        private BSThreadInfo[] mThreadInfo;
        private Thread[] mThread;

        public List<BSConnection> ConnectionList;

        public void Start()
        {
            if (isRunning || CoreThread != null) return;

            isRunning = true;
            CoreThread = new Thread(WorkLoop) { IsBackground = true };
            CoreThread.Start();
        }

        public void Stop()
        {
            isRunning = false;

            lock (_threadLocker){ Monitor.PulseAll(_threadLocker); }


        }

        private void WorkLoop()
        {
            try
            {
                Time = _stopWatch.ElapsedMilliseconds;

                if (!Settings.Multithreaded) Settings.ThreadLimit = 1;

                mThread = new Thread[Settings.ThreadLimit];
                mThreadInfo = new BSThreadInfo[Settings.ThreadLimit];

                for (int a = 0; a < mThreadInfo.Length; a++)
                {
                    mThreadInfo[a] = new BSThreadInfo(a);
                }

                // Server System Boot
                //
                //
                StartCore();

                if (Settings.Multithreaded)
                {
                    for (int a = 0; a < mThreadInfo.Length; a++)
                    {
                        mThread[a] = new Thread(() => ThreadLoop(mThreadInfo[a]));
                        mThread[a].IsBackground = true;
                        mThread[a].Start();
                    }
                }

                // Server Network Boot
                //
                //

                try
                {
                    while (isRunning) // Core Thread Loop
                    {
                        Time = _stopWatch.ElapsedMilliseconds;
                    }
                }
                catch (Exception ex)
                {
                    FrmMain.Enqueue(ex);

                    var _st = new StackTrace(ex, true);
                    var _frame = _st.GetFrame(0);
                    var _line = _frame.GetFileLineNumber();

                    File.AppendAllText(@".\Error.txt", string.Format("[{0}] {1} at line {2}{3}", Now, ex, _line, Environment.NewLine));
                }

                // Server System Stop
                //
                //


            }
            catch (Exception ex)
            {
                FrmMain.Enqueue(ex);

                var _st = new StackTrace(ex, true);
                var _frame = _st.GetFrame(0);
                var _line = _frame.GetFileLineNumber();

                File.AppendAllText(@".\Error.txt", string.Format("[{0}] {1} at line {2}{3}", Now, ex, _line, Environment.NewLine));
            }
        }

        private void StartCore()
        {

        }

        private void ThreadLoop(BSThreadInfo Info)
        {
            Info.Stop = false;
            long InitTime = Time;

            try
            {

            }
            catch (Exception ex)
            {
                if (ex is ThreadInterruptedException) return;

                FrmMain.Enqueue(ex);
                File.AppendAllText(@".\Error.txt", string.Format("[{0}] {1}{2}", Now, ex, Environment.NewLine));
            }
        }
    }
    
}
