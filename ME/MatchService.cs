using ME.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ME
{
    sealed class MatchService
    {
        private static readonly Lazy<MatchService> lazy = new Lazy<MatchService>(() => new MatchService());

        public static MatchService Instance { get { return lazy.Value; } }
        private readonly LinkedList<Order> BuyOrders = new LinkedList<Order>();
        private readonly LinkedList<Order> SellOrders = new LinkedList<Order>();
        long COUNTER;
        private MatchService()
        {
            COUNTER = 1000;//new Random().Next(1000, 1001);
            //BuyOrders = new LinkedList<Order>();
            //SellOrders = new LinkedList<Order>();
        }

        public long getOrderID()
        {
            return Interlocked.Increment(ref COUNTER);
        }

        //Self match allowed
        public Order PlaceMyOrder(Order order)
        {
            if (order.Side == 1)
                BuyOrders.AddLast(order);
            return order;
        }

    }
}
