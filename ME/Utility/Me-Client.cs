using ME.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ME.Utility
{
    class Me_Client
    {
        private static readonly Random random = new Random();
        private static readonly string endpoint = "http://localhost:8080/PlaceOrder";
        public static List<Order> getRandomOrders(long count)
        {
           
            List<Order> orders = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                // if(random.Next()%2==0)
                orders.Add( new Order { Rate = RandomDecimalBetween(1, 2), Type = OrderType.Limit, Side = (random.Next() % 2 == 0) ? OrderSide.Sell : OrderSide.Buy, UserID = 250250, Volume = RandomDecimalBetween(1, 2) });
                
            }
            return orders;
        }
        public static void PlaceAllOrder(List<Order> orders,bool isParallel=false)
        {
            if (isParallel)
                Parallel.ForEach(orders, (order) => PlaceAnOrder(order));
            else
                orders.ForEach((order) => PlaceAnOrder(order));
        }
        private static void PlaceAnOrder(Order order)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.UploadString(new Uri(endpoint), JsonConvert.SerializeObject(order));
               // Console.WriteLine(res);
            }
        }
        private static decimal RandomDecimalBetween(decimal minValue, decimal maxValue)
        {
            var next = (decimal)random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}
