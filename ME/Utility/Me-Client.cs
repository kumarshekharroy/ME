﻿using ME.Model;
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
        private static int deciaml_precision = 4;
        private static readonly string endpoint_SingleOrder = "http://localhost:8080/PlaceOrder";
        private static readonly string endpoint_BulkOrder = "http://localhost:8080/PlaceBulkOrders";

        private static decimal RandomDecimalBetween(decimal minValue, decimal maxValue)
        {
            var next = (decimal)random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
        public static List<Order> getRandomOrders(long count)
        {

            List<Order> orders = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                // if(random.Next()%2==0)
                orders.Add(new Order { Rate = Math.Round(RandomDecimalBetween(0, 1), deciaml_precision), Type = OrderType.Limit, Side = (random.Next() % 2 == 0) ? OrderSide.Sell : OrderSide.Buy, UserID = 250250, Volume = Math.Round(RandomDecimalBetween(0, 1), deciaml_precision) });

            }
            return orders;
        }






        public static void PlaceAllOrder(List<Order> orders, bool isParallel = false, bool isBulk = false)
        {
            if (isBulk)
                PlaceBulkOrder(orders);

            else if (isParallel)
                Parallel.ForEach(orders, (order) => PlaceAnOrder(order));
            else
                orders.ForEach((order) => PlaceAnOrder(order));
        }
        private static void PlaceBulkOrder(List<Order> orders)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    var payload = JsonConvert.SerializeObject(orders);
                    client.UploadString(new Uri(endpoint_BulkOrder), payload);
                    // Console.WriteLine(res);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        private static void PlaceAnOrder(Order order)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.UploadString(new Uri(endpoint_SingleOrder), JsonConvert.SerializeObject(order));
                // Console.WriteLine(res);
            }
        }
    }
}
