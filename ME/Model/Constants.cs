using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{ 
    public enum OrderSide
    {
        None=0,
        Buy = 1,
        Sell = 2
    }

    public enum OrderType
    {
        None = 0,
        Limit = 1,
        StopLimit = 2,
        Market = 3,
        StopMarket = 4,
        //StopLimitToLimit=5,
    }

    public enum OrderStatus
    {
        None = 0, 
        Accepted = 1,
        PartiallyFilled = 2,
        FullyFilled = 3,
        Rejected = 4,
        CancellationPending = 5,
        CancellationAccepted = 6,
        CancellationRejected = 7,
    }

    public enum OrderTimeInForce
    {
        GTC = 0,
        DO = 1,
        IOC = 2,
        FOK = 3, 
    }

    public static class EnumHelper
    {
        public static bool getOrderStatusBool(OrderStatus orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatus.None: 
                    return true;
                case OrderStatus.Accepted:
                    return false;
                case OrderStatus.PartiallyFilled:
                    return false;
                case OrderStatus.FullyFilled:
                    return true;
                case OrderStatus.Rejected:
                    return true;
                case OrderStatus.CancellationPending:
                    return false;
                case OrderStatus.CancellationRejected:
                    return false;
                case OrderStatus.CancellationAccepted:
                    return true;
                default:
                    return false;
            }
        }
    }
}
