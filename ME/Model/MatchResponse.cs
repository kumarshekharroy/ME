using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{
  public  class MatchResponse
    {
        public string Pair { get; set; }
        public List<Order> UpdatedBuyOrders { get; set; }
        public List<Order> UpdatedSellOrders { get; set; }
        public List<Trade> NewTrades { get; set; }
    }
}
