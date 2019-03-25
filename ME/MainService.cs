using ME.Model;
using ME.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ME
{
    sealed class MainService
    {
        private readonly int deciaml_precision;
        private readonly decimal dust_size;
        private decimal current_Market_Price = 0M;
        private readonly object buyLock = new object();
        private readonly object sellLock = new object();
        private readonly object buyLock_stopLimit = new object();
        private readonly object sellLock_stopLimit = new object();
        // private readonly WCTicker WCTicker_Instance= WCTicker.Instance;


        public MainService(int Precision = 8, decimal DustSize = 0.000001M)
        {
            this.deciaml_precision = Precision;
            this.dust_size = DustSize;

            Task.Run(() => MatchMyOrder_CornJob());
            Trades.ItemAdded += new Custom_ConcurrentQueue<Trade>.ItemAddedDelegate(newTradeNotification);
            MatchResponses.ItemAdded += new Custom_ConcurrentQueue<MatchResponse>.ItemAddedDelegate(newMatchResponsesNotification);
        }

        //private static readonly Lazy<MainService> lazy = new Lazy<MainService>(() => new MainService()); 
        //public static MainService Instance { get { return lazy.Value; } }

        private readonly ConcurrentQueue<Order> PendingOrderQueue = new ConcurrentQueue<Order>();

        private readonly SortedDictionary<decimal, LinkedList<Order>> BuyOrdersDict = new SortedDictionary<decimal, LinkedList<Order>>();
        private readonly SortedDictionary<decimal, LinkedList<Order>> SellOrdersDict = new SortedDictionary<decimal, LinkedList<Order>>();


        private readonly SortedDictionary<decimal, LinkedList<Order>> StopLimit_BuyOrdersDict = new SortedDictionary<decimal, LinkedList<Order>>();
        private readonly SortedDictionary<decimal, LinkedList<Order>> StopLimit_SellOrdersDict = new SortedDictionary<decimal, LinkedList<Order>>();


        private readonly Custom_ConcurrentQueue<Trade> Trades = new Custom_ConcurrentQueue<Trade>();
        private readonly Custom_ConcurrentQueue<MatchResponse> MatchResponses = new Custom_ConcurrentQueue<MatchResponse>();
        private readonly Statistic statistic = new Statistic();




        public long getOrderID()
        {
            return Interlocked.Increment(ref ME_Gateway.Instance.OrderID);
        }
        public long getTradeID()
        {
            return Interlocked.Increment(ref ME_Gateway.Instance.TradeID);
        }


        public void newTradeNotification(Trade trade)
        {
            var SellActivationTask = Task.Run(() =>
            {
                lock (sellLock_stopLimit)
                {
                    var shouldContinue = false;
                    do
                    {

                        var stopprice = StopLimit_SellOrdersDict.Keys.Reverse().FirstOrDefault();
                        if (stopprice < trade.Rate)
                            break;  //Break as No StopLimitSell Order above Current Trading price;

                        foreach (var order in StopLimit_SellOrdersDict[stopprice])
                        {
                            order.Type = OrderType.StopLimitToLimit;
                            PendingOrderQueue.Enqueue(order);
                        }

                        StopLimit_SellOrdersDict.Remove(stopprice);
                        shouldContinue = true;
                    } while (shouldContinue);
                }

            });
            var BuyActivationTask = Task.Run(() =>
            {
                lock (buyLock_stopLimit)
                {
                    var shouldContinue = false;
                    do
                    {
                        var stopprice = StopLimit_BuyOrdersDict.Keys.FirstOrDefault();
                        if (stopprice == 0 || stopprice > trade.Rate)
                            break;  //Break as No StopLimitBuy Order below Current Trading price;

                        foreach (var order in StopLimit_BuyOrdersDict[stopprice])
                        {
                            order.Type = OrderType.StopLimitToLimit;
                            PendingOrderQueue.Enqueue(order);
                        }

                        StopLimit_BuyOrdersDict.Remove(stopprice);

                        shouldContinue = true;
                    } while (shouldContinue);

                }
            });
            var NotificationTask = Task.Run(() =>
            {
                WC_TradeTicker.PushTicker(trade.Pair, trade);
            });
        }
        public void newMatchResponsesNotification(MatchResponse matchResponse)
        {
            //send push Notification using socket;
            WC_MatchTicker.PushTicker(matchResponse.Pair, matchResponse);

        }


        //public proerties
        public Statistic GetStats
        {
            get
            {
                this.statistic.OpenOrders = this.OpenOrdersCount;
                this.statistic.Book = this.BookCount;
                this.statistic.TPS = (int)((this.statistic.Submission + this.statistic.Trades + this.statistic.Cancellation) / (DateTime.UtcNow - this.statistic.InitTime).TotalSeconds);
                return this.statistic;
            }
        }
        public int OpenOrdersCount
        {
            get
            {
                return this.AllBuyOrders.Count + this.AllSellOrders.Count;
            }
        }
        public int BookCount
        {
            get
            {
                return this.BuyOrdersDict.Count + this.SellOrdersDict.Count;
            }
        }
        public ConcurrentQueue<Order> AllPendingOrderQueue
        {
            get
            {
                return this.PendingOrderQueue;
            }
        }
        public List<Order> AllSellOrders
        {
            get
            {
                lock (sellLock)
                    return this.SellOrdersDict.Values.SelectMany(x => x).OrderBy(x => x.ID).ToList();
            }
        }
        public List<Order> AllBuyOrders
        {
            get
            {
                lock (buyLock)
                    return this.BuyOrdersDict.Values.SelectMany(x => x).OrderBy(x => x.ID).ToList();
            }
        }
        public ConcurrentQueue<Trade> AllTrades
        {
            get
            {
                return this.Trades;
            }
        }
        public ConcurrentQueue<MatchResponse> AllMatchResponses
        {
            get
            {
                return this.MatchResponses;
            }
        }


        //Self match allowed
        public Order PlaceMyOrder(Order order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.Pair))
                return default(Order);

            order.Rate = order.Rate.TruncateDecimal(deciaml_precision);
            order.Volume = order.Rate.TruncateDecimal(deciaml_precision);
            order.Stop = order.Stop.TruncateDecimal(deciaml_precision);
            if (order.Rate <= 0 || order.Volume <= 0 || (order.Rate * order.Volume).TruncateDecimal(deciaml_precision) <= 0
                || (order.Type == OrderType.StopLimit && ((order.Stop <= 0) || (order.Side == OrderSide.Sell && order.Stop < order.Rate) || (order.Side == OrderSide.Buy && order.Stop < order.Rate)))
                )
            {
                order.Status = OrderStatus.Rejected;
                return order;
            }
            order.ID = this.getOrderID();
            order.Status = OrderStatus.Accepted;
            order.AcceptedOn = DateTime.UtcNow;
            order.PendingVolume = order.Volume;


            if (order.Type == OrderType.StopLimit)
            {
                //As StopLimit Buy Price is less or Sell Price is higher than market price so treat it as LIMIT Order.
                if ((order.Side == OrderSide.Sell && order.Stop >= current_Market_Price) || (order.Side == OrderSide.Buy && order.Stop <= current_Market_Price))
                {
                    order.Type = OrderType.Limit;
                    PendingOrderQueue.Enqueue(order);
                }
                else
                {
                    if (order.Side == OrderSide.Buy)
                    {
                        lock (buyLock_stopLimit)
                        {
                            LinkedList<Order> orderList;
                            if (StopLimit_BuyOrdersDict.TryGetValue(order.Stop, out orderList))
                                orderList.AddLast(order);
                            else
                                StopLimit_BuyOrdersDict[order.Stop] = new LinkedList<Order>(new List<Order> { order });
                        }

                    }
                    else if (order.Side == OrderSide.Sell)
                    {
                        lock (sellLock_stopLimit)
                        {
                            LinkedList<Order> orderList;
                            if (StopLimit_SellOrdersDict.TryGetValue(order.Stop, out orderList))
                                orderList.AddLast(order);
                            else
                                StopLimit_SellOrdersDict[order.Stop] = new LinkedList<Order>(new List<Order> { order }); 
                        }
                    } 
                }
            }
            else
            {
                PendingOrderQueue.Enqueue(order);
            }

            this.statistic.inc_submission();
            return order;
        }
        public Order CancelMyOrder(Order order)
        {
            if (order == null)
                return default(Order);
            else if (order.ID == 0 || order.Side == 0)
            {
                order.Status = OrderStatus.CancellationRejected;
                return order;
            }

            order.Status = OrderStatus.CancellationPending;
            PendingOrderQueue.Enqueue(order);
            this.statistic.inc_cancellation();
            return order;
        }

        public List<Order> PlaceMyBulkOrder(List<Order> orders)
        {
            Parallel.ForEach(orders, (order) => this.PlaceMyOrder(order));
            return orders;
        }
        public void MatchMyOrder_CornJob()
        {
            Order order;
            while (true)
            {
                if (!PendingOrderQueue.IsEmpty)
                {

                    if (PendingOrderQueue.TryDequeue(out order))
                        try
                        {
                            MatchMyOrder(order);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception => {ex.Message}");
                        }
                }
                else
                    Task.Delay(100).Wait();
            }

        }

        //Self match allowed
        public MatchResponse MatchMyOrder(Order order)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var CurrentTime = DateTime.UtcNow;
            MatchResponse response = new MatchResponse
            {
                Pair = order.Pair,
                UpdatedBuyOrders = new List<Order>(),
                UpdatedSellOrders = new List<Order>(),
                NewTrades = new List<Trade>()
            };


            if (order.Side == OrderSide.Buy)
            {
                //Cancellation Reqest 
                if (order.Status == OrderStatus.CancellationPending && order.ID != 0)
                {
                    bool isfound = false;
                    Parallel.ForEach(BuyOrdersDict.Values, (gropued_list, loopState) =>
                    {
                        var orderToBeCancelled = gropued_list.FirstOrDefault(x => x.ID == order.ID);
                        if (orderToBeCancelled != null)
                        {
                            isfound = true;
                            loopState.Break();


                            lock (buyLock)
                            {
                                if (gropued_list.Count <= 1)
                                    BuyOrdersDict.Remove(orderToBeCancelled.Rate);
                                else
                                    gropued_list.Remove(orderToBeCancelled);
                            }
                            orderToBeCancelled.Status = OrderStatus.CancellationAccepted;
                            response.UpdatedBuyOrders.Add(orderToBeCancelled);
                            loopState.Stop();
                        }
                    });
                    if (!isfound)
                        Parallel.ForEach(StopLimit_BuyOrdersDict.Values, (gropued_list, loopState) =>
                        {
                            var orderToBeCancelled = gropued_list.FirstOrDefault(x => x.ID == order.ID);
                            if (orderToBeCancelled != null)
                            {
                                loopState.Break();
                                lock (buyLock_stopLimit)
                                {
                                    if (gropued_list.Count <= 1)
                                        StopLimit_BuyOrdersDict.Remove(orderToBeCancelled.Stop);
                                    else
                                        gropued_list.Remove(orderToBeCancelled);
                                }
                                orderToBeCancelled.Status = OrderStatus.CancellationAccepted;
                                response.UpdatedBuyOrders.Add(orderToBeCancelled);
                                loopState.Stop();
                            }
                        });
                }
                else
                {
                    response.UpdatedBuyOrders.Add((Order)order.Clone());
                    while (order.PendingVolume > 0 && SellOrdersDict.Count > 0)
                    {
                        var PossibleMatches_ = SellOrdersDict.FirstOrDefault();
                        if (PossibleMatches_.Key > order.Rate)
                            break;  //Break as No Match Found for New

                        var sellOrder_Node = PossibleMatches_.Value.First;
                        while (sellOrder_Node != null)
                        {
                            var next_Node = sellOrder_Node.Next;

                            var trade = new Trade(CurrentTime);

                            var sellOrder = sellOrder_Node.Value;
                            if ((sellOrder.PendingVolume * sellOrder.Rate).TruncateDecimal(deciaml_precision) < dust_size)
                            {
                                sellOrder.Status = OrderStatus.Rejected;
                                response.UpdatedSellOrders.Add(sellOrder);
                                this.statistic.DustOrders++;
                            }
                            else
                            {
                                if (sellOrder.PendingVolume >= order.PendingVolume)//CompleteMatch_New  PartiallyMatch Existing
                                {
                                    sellOrder.PendingVolume -= order.PendingVolume;
                                    sellOrder.Status = sellOrder.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                                    sellOrder.ModifiedOn = CurrentTime;
                                    order.PendingVolume = 0;
                                    order.Status = OrderStatus.FullyFilled;
                                    order.ModifiedOn = CurrentTime;

                                    trade.ID = this.getTradeID();
                                    trade.Pair = order.Pair;
                                    trade.OrderID_Buy = order.ID;
                                    trade.OrderID_Sell = sellOrder.ID;
                                    trade.Side = order.Side;
                                    trade.Rate = sellOrder.Rate;
                                    trade.Volume = order.Volume;
                                    trade.UserID_Buyer = order.UserID;
                                    trade.UserID_Seller = sellOrder.UserID;
                                }
                                else //CompleteMatch_Existing  PartiallyMatch New
                                {
                                    order.PendingVolume -= sellOrder.PendingVolume;
                                    order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                                    order.ModifiedOn = CurrentTime;
                                    sellOrder.PendingVolume = 0;
                                    sellOrder.Status = OrderStatus.FullyFilled;
                                    sellOrder.ModifiedOn = CurrentTime;


                                    trade.ID = this.getTradeID();
                                    trade.Pair = order.Pair;
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

                                Trades.Enqueue(trade);
                                current_Market_Price = trade.Rate;
                            }

                            if (EnumHelper.getOrderStatusBool(sellOrder.Status))
                            {
                                if (PossibleMatches_.Value.Count == 1)
                                {
                                    lock (sellLock)
                                        SellOrdersDict.Remove(sellOrder.Rate); //The Order was Only Order at the given rate;
                                    break;//Break from foreach as No Pending Order at given rate
                                }
                                else
                                {
                                    lock (sellLock)
                                        PossibleMatches_.Value.Remove(sellOrder_Node);
                                }
                            }
                            this.statistic.Trades++;

                            if (order.Status == OrderStatus.FullyFilled)
                                break;  //Break as Completely Matched New

                            sellOrder_Node = next_Node;

                        }
                    }
                    //lock (buyLock)
                    if (!EnumHelper.getOrderStatusBool(order.Status))
                    {
                        LinkedList<Order> orderList;
                        if (BuyOrdersDict.TryGetValue(order.Rate, out orderList))
                            lock (buyLock)
                                orderList.AddLast(order);
                        else
                            lock (buyLock)
                                BuyOrdersDict[order.Rate] = new LinkedList<Order>(new List<Order> { order });
                    }
                }
            }
            else if (order.Side == OrderSide.Sell)
            {
                if (order.Status == OrderStatus.CancellationPending && order.ID != 0)
                {
                    bool isfound = false;

                    Parallel.ForEach(SellOrdersDict.Values, (gropued_list, loopState) =>
                    {
                        var orderToBeCancelled = gropued_list.FirstOrDefault(x => x.ID == order.ID);
                        if (orderToBeCancelled != null)
                        {
                            isfound = true;
                            loopState.Break();

                            lock (sellLock)
                            {
                                if (gropued_list.Count <= 1)
                                    SellOrdersDict.Remove(orderToBeCancelled.Rate);
                                else
                                    gropued_list.Remove(orderToBeCancelled);
                            }

                            orderToBeCancelled.Status = OrderStatus.CancellationAccepted;
                            response.UpdatedSellOrders.Add(orderToBeCancelled);
                            loopState.Stop();
                        }
                    });
                    if (!isfound)
                        Parallel.ForEach(StopLimit_SellOrdersDict.Values, (gropued_list, loopState) =>
                          {
                              var orderToBeCancelled = gropued_list.FirstOrDefault(x => x.ID == order.ID);
                              if (orderToBeCancelled != null)
                              {
                                  isfound = true;
                                  loopState.Break();

                                  lock (sellLock_stopLimit)
                                  {
                                      if (gropued_list.Count <= 1)
                                          StopLimit_SellOrdersDict.Remove(orderToBeCancelled.Stop);
                                      else
                                          gropued_list.Remove(orderToBeCancelled);
                                  }
                                  orderToBeCancelled.Status = OrderStatus.CancellationAccepted;
                                  response.UpdatedSellOrders.Add(orderToBeCancelled);
                                  loopState.Stop();
                              }
                          });

                }
                else
                {
                    response.UpdatedSellOrders.Add((Order)order.Clone());

                    while (order.PendingVolume > 0 && BuyOrdersDict.Count > 0)
                    {
                        var PossibleMatches_ = BuyOrdersDict.Reverse().FirstOrDefault();
                        if (PossibleMatches_.Key < order.Rate)
                            break;  //Break as No Match Found for New

                        var buyOrder_Node = PossibleMatches_.Value.First;
                        while (buyOrder_Node != null)
                        {
                            var next_Node = buyOrder_Node.Next;

                            var trade = new Trade(CurrentTime);

                            var buyOrder = buyOrder_Node.Value;
                            if ((buyOrder.PendingVolume * buyOrder.Rate).TruncateDecimal(deciaml_precision) < dust_size)
                            {
                                buyOrder.Status = OrderStatus.Rejected;
                                response.UpdatedSellOrders.Add(buyOrder);
                                this.statistic.DustOrders++;
                            }
                            else
                            {
                                if (buyOrder.PendingVolume >= order.PendingVolume)//CompleteMatch_New  PartiallyMatch Existing
                                {
                                    buyOrder.PendingVolume -= order.PendingVolume;
                                    buyOrder.Status = buyOrder.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                                    buyOrder.ModifiedOn = CurrentTime;
                                    order.PendingVolume = 0;
                                    order.Status = OrderStatus.FullyFilled;
                                    order.ModifiedOn = CurrentTime;

                                    trade.ID = this.getTradeID();
                                    trade.Pair = order.Pair;
                                    trade.OrderID_Buy = buyOrder.ID;
                                    trade.OrderID_Sell = order.ID;
                                    trade.Side = order.Side;
                                    trade.Rate = buyOrder.Rate;
                                    trade.Volume = order.Volume;
                                    trade.UserID_Buyer = buyOrder.UserID;
                                    trade.UserID_Seller = order.UserID;
                                }
                                else //CompleteMatch_Existing  PartiallyMatch New
                                {
                                    order.PendingVolume -= buyOrder.PendingVolume;
                                    order.Status = order.PendingVolume <= 0 ? OrderStatus.FullyFilled : OrderStatus.PartiallyFilled;
                                    order.ModifiedOn = CurrentTime;
                                    buyOrder.PendingVolume = 0;
                                    buyOrder.Status = OrderStatus.FullyFilled;
                                    buyOrder.ModifiedOn = CurrentTime;


                                    trade.ID = this.getTradeID();
                                    trade.Pair = order.Pair;
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


                                Trades.Enqueue(trade);
                                current_Market_Price = trade.Rate;
                            }
                            if (EnumHelper.getOrderStatusBool(buyOrder.Status))
                            {
                                if (PossibleMatches_.Value.Count == 1)
                                {
                                    lock (buyLock)
                                        BuyOrdersDict.Remove(buyOrder.Rate); //The Order was Only Order at the given rate;
                                    break;//Break from foreach as No Pending Order at given rate
                                }
                                else
                                {
                                    lock (buyLock)
                                        PossibleMatches_.Value.Remove(buyOrder_Node);
                                }

                            }

                            this.statistic.Trades++;
                            if (order.Status == OrderStatus.FullyFilled)
                                break;  //Break as Completely Matched New

                            buyOrder_Node = next_Node;
                        }
                    }
                    //lock (sellLock)
                    if (!EnumHelper.getOrderStatusBool(order.Status))
                    {
                        LinkedList<Order> orderList;
                        if (SellOrdersDict.TryGetValue(order.Rate, out orderList))
                            lock (sellLock)
                                orderList.AddLast(order);
                        else
                            lock (sellLock)
                                SellOrdersDict[order.Rate] = new LinkedList<Order>(new List<Order> { order });
                    }
                }
            }

            MatchResponses.Enqueue(response);
            this.statistic.inc_processed();
            //stopwatch.Stop(); 
            // Console.WriteLine($"{order.ID} => {stopwatch.ElapsedMilliseconds}");
            return response;
        }


    }
}
