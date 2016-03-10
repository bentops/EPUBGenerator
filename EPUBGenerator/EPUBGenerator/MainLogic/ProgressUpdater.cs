using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class ProgressUpdater
    {
        private BackgroundWorker backGroundWorker { get; set; }
        private DoWorkEventArgs doWorkEventArgs { get; set; }

        private String cancellationMessage;

        public bool Initialized { get; private set; }
        public int Counter { get; private set; }
        public int Total { get; private set; }
        public int Percentage
        {
            get
            {
                if (Total == 0) return 100;
                return Counter * 100 / Total;
            }
        }

        public ProgressUpdater(BackgroundWorker bw, DoWorkEventArgs e)
        {
            if (bw == null || e == null)
                throw new NullReferenceException("Progress Updater: null bw or e.");

            backGroundWorker = bw;
            doWorkEventArgs = e;

            backGroundWorker.WorkerReportsProgress = true;
            backGroundWorker.WorkerSupportsCancellation = true;
        }

        public void Initialize(int total)
        {
            Initialize(0, total);
        }

        public void Initialize(int start, int total)
        {
            Counter = start;
            Total = total;
            Initialized = true;
            Report();
        }

        public void Increment()
        {
            Increment(1);
        }

        public void Increment(int num)
        {
            Counter += num;
            Report();
        }

        public void Report()
        {
            if (!Initialized)
                throw new InvalidOperationException("ProgressUpdater is not initialized.");

            if (backGroundWorker.CancellationPending)
            {
                doWorkEventArgs.Cancel = true;
                throw new OperationCanceledException(cancellationMessage);
            }
            backGroundWorker.ReportProgress(Percentage);
            //Console.WriteLine("REPORT: " + Percentage + "(" + Counter + "/" + Total + ")");
        }

        public void Cancel()
        {
            Cancel(null);
        }

        public void Cancel(String message)
        {
            cancellationMessage = message;
            backGroundWorker.CancelAsync();
        }
    }
}
