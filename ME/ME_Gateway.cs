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
        }
        private readonly object listedPairs_CreationLock = new object();
        private static readonly Lazy<ME_Gateway> lazy = new Lazy<ME_Gateway>(() => new ME_Gateway());
        public static ME_Gateway Instance { get { return lazy.Value; } }
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
                listedPairs.ForEach((_pair) => { if (remove) { _listedPairs.TryRemove(_pair, out MainService mainService); } else { _listedPairs[_pair] = new MainService { }; } });
            }
            else
                if (remove) { _listedPairs.TryRemove(pair, out MainService mainService); } else { _listedPairs[pair] = new MainService { }; }

        }
    }
}
