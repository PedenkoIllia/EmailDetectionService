using log4net;
using System;
using System.Threading;

namespace EmailDetectionService
{
    public class JobController
    {
        protected static readonly ILog _log = LogManager.GetLogger("EmailProcessor");

        private int maxJobCount = 0;
        private object sync = new object();
        private bool isRunning;
        private int currentJobCount;

        public JobController(int maxJobCount = 10)
        {
            this.isRunning = false;
            int workerThreads, completionPortThread;
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThread);

            if (maxJobCount < 1) 
                maxJobCount = 1; 

            this.maxJobCount = maxJobCount;
        }

        public bool IsRunning { get => isRunning; }

        public void Start() => isRunning = true;

        public void Stop()
        {
            isRunning = false;

            lock (sync)
            {
                while (currentJobCount > 0)
                    Monitor.Wait(sync);
            }
        }
        public void StartNewProcess(Action newAction)
        {
            lock (sync)
            {
                while (currentJobCount >= maxJobCount)
                {
                    Monitor.Wait(sync);
                }

                if (!isRunning)
                    return;

                currentJobCount++;
            }

            ThreadPool.QueueUserWorkItem(x =>
            {
                try
                {
                    newAction();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
                finally
                {
                    lock (sync)
                    {
                        this.currentJobCount--;
                        Monitor.Pulse(sync);
                    }
                }
            });
        }
    }
}
