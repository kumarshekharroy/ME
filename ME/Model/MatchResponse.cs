using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{
    class MatchResponse
    {
        public List<Order> UpdatedBuyOrders { get; set; }
        public List<Order> UpdatedSellOrders { get; set; }
        public List<Trade> NewTrades { get; set; }
    }
}
