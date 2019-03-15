using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{ 
  public  class Trade
    {
        public long ID { get; set; }
        public long UserID_Buyer { get; set; }
        public long UserID_Seller { get; set; }
        public long OrderID_Buy { get; set; }
        public long OrderID_Sell { get; set; }
        public OrderSide Side { get; set; } 
        public decimal Volume { get; set; }
        public decimal Rate { get; set; }  
    }
}
