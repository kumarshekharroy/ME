using ME.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME
{
    sealed class ME_Gateway
    {


        private ME_Gateway()
        {
            this.OrderID = 1000;
            //Task.Run(() =>
            //{
            //    while (!MatchResponseQueue.IsCompleted)
            //    {
            //        MatchResponse matchResponse;
            //        if (MatchResponseQueue.TryTake(out matchResponse, timeout: TimeSpan.FromMilliseconds(10000)))
            //            try
            //            {
            //                WC_MatchTicker.PushTicker(matchResponse.Pair, matchResponse);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine($"Exception => {ex.Message}");
            //            }
            //    }
            //});
            //Task.Run(() =>
            //{
            //    while (!TradeQueue.IsCompleted)
            //    {
            //        Trade trade;
            //        if (TradeQueue.TryTake(out trade, timeout: TimeSpan.FromMilliseconds(10000)))
            //            try
            //            {
            //                WC_TradeTicker.PushTicker(trade.Pair, trade);
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine($"Exception => {ex.Message}");
            //            }
            //    }
            //});
           
        }
        private readonly object listedPairs_CreationLock = new object();
        private static readonly Lazy<ME_Gateway> lazy = new Lazy<ME_Gateway>(() => new ME_Gateway());
        public static ME_Gateway Instance { get { return lazy.Value; } }

        public BlockingCollection<MatchResponse> MatchResponseQueue = new BlockingCollection<MatchResponse>();
        public BlockingCollection<Trade> TradeQueue = new BlockingCollection<Trade>();

        private readonly ConcurrentDictionary<string, MainService> _listedPairs = new ConcurrentDictionary<string, MainService>();
        public long OrderID, TradeID;
        public List<string> listedPairs { get { return this._listedPairs.Keys.ToList(); } }
        public MainService this[string pair, int precision = 8, decimal dustSize = 0.000001M]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(pair))
                    throw new InvalidOperationException("Pair can't be null.");
                MainService ME_Service;
                if (!_listedPairs.TryGetValue(pair, out ME_Service))
                {
                    lock (listedPairs_CreationLock)
                    {

                        if (!_listedPairs.TryGetValue(pair, out ME_Service))
                        {
                            ME_Service = new MainService(precision, dustSize);
                            if (!_listedPairs.TryAdd(pair, ME_Service))
                                throw new InvalidOperationException("Failed to instantiate ME pair.Please Retry.");
                        }

                    }
                }
                return ME_Service;
            }
        }

        public void ResetPair(string pair = "", bool remove = true)
        {
            if (string.IsNullOrWhiteSpace(pair))
            {
                if (remove)
                    _listedPairs.Clear();
                else
                    listedPairs.ForEach((_pair) => { _listedPairs[_pair] = new MainService { }; });
                TradeID = 0;
                OrderID = 1000;
            }
            else
                if (remove) { _listedPairs.TryRemove(pair, out MainService mainService); } else { _listedPairs[pair] = new MainService { }; }

        }
    }
}
