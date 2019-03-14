using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{ 
    public enum OrderSide
    {
        Buy = 1,
        Sell = 2
    }

    public enum OrderType
    {
        Limit = 1,
        StopLimit = 2,
        Market = 3,
        StopMarket = 4
    }

    public enum OrderStatus
    {
        None = 1,
        Accepted = 2,
        PartiallyFilled = 3,
        FullyFilled = 4,
        Rejected = 5,
        Cancelled = 6
    }

}
