using ME.Model;
using System;
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
        private readonly List<Order> BuyOrders = new List<Order>();
        private readonly List<Order> SellOrders = new List<Order>();
        private readonly List<Trade> Trades = new List<Trade>();
        private readonly Queue<MatchResponse> Responses = new Queue<MatchResponse>();
        long OrderID,TradeID;
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

        //Self match allowed
        public MatchResponse PlaceMyOrder(Order order)
        {
            order.ID = this.getOrderID();
            order.Status = OrderStatus.Accepted;
            order.AcceptedOn = DateTime.UtcNow;

            return MatchMyOrder(order);
        }

        //Self match allowed
        public MatchResponse MatchMyOrder(Order order)
        {
            var CurrentTime = DateTime.UtcNow;
            var response = new MatchResponse { UpdatedBuyOrders = new List<Order>(), UpdatedSellOrders = new List<Order>(), NewTrades = new List<Trade>() };
            if (order.Side == OrderSide.Buy)
            {
                var PossibleMatches = SellOrders.Where(x => x.Type == order.Type && x.Rate <= order.Rate && (x.Status == OrderStatus.Accepted || x.Status == OrderStatus.PartiallyFilled)).OrderBy(x => x.Rate).ThenBy(x => x.ID).ToList();
                foreach (var sellOrder in PossibleMatches)
                {
                    var trade = new Trade();
                     
                    if (sellOrder.PendingVolume >= order.PendingVolume)//CompleteMatch
                    {
                        response.UpdatedBuyOrders.Add(order);

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
                        sellOrder.PendingVolume = 0;
                        sellOrder.Status = OrderStatus.FullyFilled;
                        sellOrder.ModifiedOn = CurrentTime;
                        order.PendingVolume-= sellOrder.PendingVolume;
                        order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        order.ModifiedOn = CurrentTime;


                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = order.ID;
                        trade.OrderID_Sell = sellOrder.ID;
                        trade.Side = order.Side;
                        trade.Rate = sellOrder.Rate;
                        trade.Volume = sellOrder.Volume;
                        trade.UserID_Buyer = order.UserID;
                        trade.UserID_Seller = sellOrder.UserID;
                    }

                    response.UpdatedBuyOrders.Add(order);
                    response.UpdatedSellOrders.Add(sellOrder);
                    response.NewTrades.Add(trade);

                    BuyOrders.Add(order);
                    Trades.Add(trade);

                    if (order.Status == OrderStatus.FullyFilled)
                        break;
                }

            }

            if (order.Side == OrderSide.Sell)
            {
                var PossibleMatches = BuyOrders.Where(x => x.Type == order.Type && x.Rate >= order.Rate && (x.Status == OrderStatus.Accepted || x.Status == OrderStatus.PartiallyFilled)).OrderByDescending(x => x.Rate).ThenBy(x => x.ID).ToList();
                foreach (var buyOrder in PossibleMatches)
                {
                    var trade = new Trade();

                    if (buyOrder.PendingVolume >= order.PendingVolume)//CompleteMatch
                    {
                        response.UpdatedSellOrders.Add(order);

                        buyOrder.PendingVolume -= order.PendingVolume;
                        buyOrder.Status = buyOrder.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        buyOrder.ModifiedOn = CurrentTime;
                        order.PendingVolume = 0;
                        order.Status = OrderStatus.FullyFilled;
                        order.ModifiedOn = CurrentTime;

                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy =buyOrder.ID ;
                        trade.OrderID_Sell = order.ID;
                        trade.Side = order.Side;
                        trade.Rate = buyOrder.Rate;
                        trade.Volume = order.Volume;
                        trade.UserID_Buyer = buyOrder.UserID;
                        trade.UserID_Seller = order.UserID;
                    }
                    else //PartialMatch
                    {
                        buyOrder.PendingVolume = 0;
                        buyOrder.Status = OrderStatus.FullyFilled;
                        buyOrder.ModifiedOn = CurrentTime;
                        order.PendingVolume -= buyOrder.PendingVolume;
                        order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                        order.ModifiedOn = CurrentTime;


                        trade.ID = this.getTradeID();
                        trade.OrderID_Buy = buyOrder.ID;
                        trade.OrderID_Sell = order.ID;
                        trade.Side = order.Side;
                        trade.Rate = buyOrder.Rate;
                        trade.Volume = buyOrder.Volume;
                        trade.UserID_Buyer = buyOrder.UserID;
                        trade.UserID_Seller = order.UserID;
                    }

                    response.UpdatedBuyOrders.Add(buyOrder);
                    response.UpdatedSellOrders.Add(order);
                    response.NewTrades.Add(trade);

                    SellOrders.Add(order);
                    Trades.Add(trade);

                    if (order.Status == OrderStatus.FullyFilled)
                        break;
                }

            }
            Responses.Enqueue(response);
            return response;
        }


    }
}
