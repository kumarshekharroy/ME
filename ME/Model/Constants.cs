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
        StopMarket = 4,
        //StopLimitToLimit=5,
    }

    public enum OrderStatus
    {
        None = 1,
        Accepted = 2,
        PartiallyFilled = 3,
        FullyFilled = 4,
        Rejected = 5,
        CancellationPending = 6,
        CancellationAccepted = 7,
        CancellationRejected = 8,
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
