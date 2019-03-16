using ME.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ME
{
    sealed class MainService
    {
        private static readonly Lazy<MainService> lazy = new Lazy<MainService>(() => new MainService());

        public static MainService Instance { get { return lazy.Value; } }
        //private readonly LinkedList<Order> BuyOrders = new LinkedList<Order>();
        //private readonly LinkedList<Order> SellOrders = new LinkedList<Order>();
        private readonly ConcurrentQueue<Order> PendingOrderQueue = new ConcurrentQueue<Order>();
        private readonly ConcurrentBag<Order> BuyOrders = new ConcurrentBag<Order>();
        private readonly ConcurrentBag<Order> SellOrders = new ConcurrentBag<Order>();
        private readonly ConcurrentBag<Trade> Trades = new ConcurrentBag<Trade>();
        private readonly ConcurrentBag<MatchResponse> MatchResponses = new ConcurrentBag<MatchResponse>();

        long OrderID, TradeID;
        private MainService()
        {
            OrderID = 1000;
        }

        public long getOrderID()
        {
            return Interlocked.Increment(ref OrderID);
        }
        public long getTradeID()
        {
            return Interlocked.Increment(ref TradeID);
        }


        //public proerties

        public ConcurrentQueue<Order> AllPendingOrderQueue
        {
            get
            {
                return this.PendingOrderQueue;
            }
        }
        public ConcurrentBag<Order> AllSellOrders
        {
            get
            {
                return this.SellOrders;
            }
        }
        public ConcurrentBag<Order> AllBuyOrders
        {
            get
            {
                return this.BuyOrders;
            }
        }

        public ConcurrentBag<Trade> AllTrades
        {
            get
            {
                return this.Trades;
            }
        }

        public ConcurrentBag<MatchResponse> AllMatchResponses
        {
            get
            {
                return this.MatchResponses;
            }
        }


        //Self match allowed
        public Order PlaceMyOrder(Order order)
        {
            order.ID = this.getOrderID();
            order.Status = OrderStatus.Accepted;
            order.AcceptedOn = DateTime.UtcNow;
            order.PendingVolume = order.Volume;
            PendingOrderQueue.Enqueue(order);
            return order;
        }

        public void MatchMyOrder_CornJob()
        {
            while (true)
            {
                if (!PendingOrderQueue.IsEmpty)
                {
                    Order order;
                    if (PendingOrderQueue.TryDequeue(out order))
                        MatchMyOrder(order);
                }
            }

        }

        //Self match allowed
        public MatchResponse MatchMyOrder(Order order)
        {

            var CurrentTime = DateTime.UtcNow;
            var response = new MatchResponse
            {
                UpdatedBuyOrders = new List<Order>(),
                UpdatedSellOrders = new List<Order>(),
                NewTrades = new List<Trade>()
            };




            // bool isMatchFound = false;
            if (order.Side == OrderSide.Buy)
            {
                response.UpdatedBuyOrders.Add((Order)order.Clone());

                var PossibleMatches = SellOrders.Where(x => x.Type == order.Type && x.Rate <= order.Rate && (x.Status == OrderStatus.Accepted || x.Status == OrderStatus.PartiallyFilled)).OrderBy(x => x.Rate).ThenBy(x => x.ID);

                foreach (var sellOrder in PossibleMatches)
                {
                    var trade = new Trade();

                    if (sellOrder.PendingVolume >= order.PendingVolume)//CompleteMatch
                    {
                        sellOrder.PendingVolume -= order.PendingVolume;
                        sellOrder.Status = sellOrder.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        sellOrder.ModifiedOn = CurrentTime;
                        order.PendingVolume = 0;
                        order.Status = OrderStatus.FullyFilled;
                        order.ModifiedOn = CurrentTime;

                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = order.ID;
                        trade.OrderID_Sell = sellOrder.ID;
                        trade.Side = order.Side;
                        trade.Rate = sellOrder.Rate;
                        trade.Volume = order.Volume;
                        trade.UserID_Buyer = order.UserID;
                        trade.UserID_Seller = sellOrder.UserID;
                    }
                    else //PartialMatch
                    {
                        order.PendingVolume -= sellOrder.PendingVolume;
                        order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        order.ModifiedOn = CurrentTime;
                        sellOrder.PendingVolume = 0;
                        sellOrder.Status = OrderStatus.FullyFilled;
                        sellOrder.ModifiedOn = CurrentTime;


                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = order.ID;
                        trade.OrderID_Sell = sellOrder.ID;
                        trade.Side = order.Side;
                        trade.Rate = sellOrder.Rate;
                        trade.Volume = sellOrder.Volume;
                        trade.UserID_Buyer = order.UserID;
                        trade.UserID_Seller = sellOrder.UserID;
                    }


                    response.UpdatedBuyOrders.Add((Order)order.Clone());
                    response.UpdatedSellOrders.Add((Order)sellOrder.Clone());
                    response.NewTrades.Add(trade);

                    Trades.Add(trade); 

                    if (order.Status == OrderStatus.FullyFilled)
                        break;
                }

                BuyOrders.Add(order);
            }

            if (order.Side == OrderSide.Sell)
            {

                response.UpdatedSellOrders.Add((Order)order.Clone());

                var PossibleMatches = BuyOrders.Where(x => x.Type == order.Type && x.Rate >= order.Rate && (x.Status == OrderStatus.Accepted || x.Status == OrderStatus.PartiallyFilled)).OrderByDescending(x => x.Rate).ThenBy(x => x.ID);

                foreach (var buyOrder in PossibleMatches)
                {
                    var trade = new Trade();

                    if (buyOrder.PendingVolume >= order.PendingVolume)//CompleteMatch
                    {
                        buyOrder.PendingVolume -= order.PendingVolume;
                        buyOrder.Status = buyOrder.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        buyOrder.ModifiedOn = CurrentTime;
                        order.PendingVolume = 0;
                        order.Status = OrderStatus.FullyFilled;
                        order.ModifiedOn = CurrentTime;

                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = buyOrder.ID;
                        trade.OrderID_Sell = order.ID;
                        trade.Side = order.Side;
                        trade.Rate = buyOrder.Rate;
                        trade.Volume = order.Volume;
                        trade.UserID_Buyer = buyOrder.UserID;
                        trade.UserID_Seller = order.UserID;
                    }
                    else //PartialMatch
                    {
                        order.PendingVolume -= buyOrder.PendingVolume;
                        order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        order.ModifiedOn = CurrentTime;
                        buyOrder.PendingVolume = 0;
                        buyOrder.Status = OrderStatus.FullyFilled;
                        buyOrder.ModifiedOn = CurrentTime;


                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = buyOrder.ID;
                        trade.OrderID_Sell = order.ID;
                        trade.Side = order.Side;
                        trade.Rate = buyOrder.Rate;
                        trade.Volume = buyOrder.Volume;
                        trade.UserID_Buyer = buyOrder.UserID;
                        trade.UserID_Seller = order.UserID;
                    }

                    response.UpdatedBuyOrders.Add((Order)buyOrder.Clone());
                    response.UpdatedSellOrders.Add((Order)order.Clone());
                    response.NewTrades.Add(trade);

                    Trades.Add(trade); 

                    if (order.Status == OrderStatus.FullyFilled)
                        break;
                }

                SellOrders.Add(order);
            }

            MatchResponses.Add(response);

            // Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
            return response;
        }


    }
}
