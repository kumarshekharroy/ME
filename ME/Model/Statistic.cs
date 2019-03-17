using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ME.Model
{
    class Statistic
    {
        //private int ;
        private static readonly object submission_lock = new object();
        private static readonly object cancellation_lock = new object(); 

        private int _Submission, _Cancellation;
        public Statistic()
        {
            this.InitTime = DateTime.UtcNow;
        }
        public int Submission { get { return _Submission; } }
        public int Trades { get; set; }
        public int Cancellation { get { return _Cancellation; } }
        public int Book { get; set; }
        public int OpenOrders { get; set; }
        public int DustOrders { get; set; }
        //public int TotalTxns { get; set; }
        //public int TxnsPerSec { get; set; } 
        public int TPS { get; set; }
        public DateTime InitTime { get; set; }
        public void inc_submission()
        {
            //lock (submission_lock)
            Interlocked.Increment(ref _Submission);
        }
        public void inc_cancellation()
        {
            //lock (cancellation_lock)
            Interlocked.Increment(ref _Cancellation);
        }
    }
}
