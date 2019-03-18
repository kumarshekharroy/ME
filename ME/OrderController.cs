
using ME.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace ME
{
    public class OrderController : ApiController
    {

        [HttpGet]
        [Route("~/Ping")]
        [Route("~/Endpoints")]
        public Response<object> Ping()
        {

            var Endpoints = new List<Uri> { new Uri("http://localhost:8080/Stats?Pair="), new Uri("http://localhost:8080/PlaceOrder"), new Uri("http://localhost:8080/CancelOrder"), new Uri("http://localhost:8080/PlaceBulkOrders"), new Uri("http://localhost:8080/GetOrders?pair=BTC&side=0"), new Uri("http://localhost:8080/GetTrades?pair=BTC"), new Uri("http://localhost:8080/Notifications?pair=BTC"), new Uri("http://localhost:8080/ResetME?pair=BTC&remove=True") };


            return new Response<object> { status = "success", message = "pong", data = Endpoints };
        }

        [HttpPost]
        [Route("~/PlaceOrder")]
        public Response<object> PlaceOrder(Order order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.Pair))
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = ME_Gateway.Instance[order.Pair].PlaceMyOrder(order);

            // Console.WriteLine(JsonConvert.SerializeObject(Response, Formatting.Indented));
            //Console.WriteLine($"\t \t \t \t {Response.ID}");
            return new Response<object> { status = "success", message = "order processed successfully", data = Response };
        }
        [HttpPost]
        [Route("~/CancelOrder")]
        public Response<object> CancelOrder(Order order)
        {
            if (order == null)
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = ME_Gateway.Instance[order.Pair].PlaceMyOrder(order);

            // Console.WriteLine(JsonConvert.SerializeObject(Response, Formatting.Indented));
            Console.WriteLine($"\t \t \t \t {Response.ID}");
            return new Response<object> { status = "success", message = "order processed successfully", data = Response };
        }
        [HttpPost]
        [Route("~/PlaceBulkOrders")]
        public Response<object> PlaceBulkOrders(BulkOrder bulkOrder)
        {
            if (bulkOrder == null || bulkOrder.orders == null || bulkOrder.orders.Count == 0)
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = ME_Gateway.Instance[bulkOrder.Pair].PlaceMyBulkOrder(bulkOrder.orders);

            // Console.WriteLine(JsonConvert.SerializeObject(Response, Formatting.Indented));
            //Console.WriteLine($"\t \t \t \t {Response.ID}");
            return new Response<object> { status = "success", message = "order processed successfully", data = Response };
        }
        [HttpGet]
        [Route("~/GetOrders")]
        public Response<object> GetOrders(string pair, int side)
        {
            if (string.IsNullOrWhiteSpace(pair))
                return new Response<object> { status = "badrequest", message = "invalid pair" };
            if (side == 1)
                return new Response<object> { status = "success", message = ME_Gateway.Instance[pair].AllBuyOrders.Count.ToString(), data = new { BuyOrders = ME_Gateway.Instance[pair].AllBuyOrders } };
            else if (side == 2)
                return new Response<object> { status = "success", message = ME_Gateway.Instance[pair].AllSellOrders.Count.ToString(), data = new { SellOrders = ME_Gateway.Instance[pair].AllSellOrders } };
            else
                return new Response<object> { status = "success", message = (ME_Gateway.Instance[pair].AllBuyOrders.Count + ME_Gateway.Instance[pair].AllSellOrders.Count).ToString(), data = new { BuyOrders = ME_Gateway.Instance[pair].AllBuyOrders, SellOrders = ME_Gateway.Instance[pair].AllSellOrders } };
        }

        [HttpGet]
        [Route("~/GetTrades")]
        public object GetTrades(string pair)
        {
            if (string.IsNullOrWhiteSpace(pair))
                return new Response<object> { status = "badrequest", message = "invalid pair" };
            //return new { Trades = MainService.Instance.AllTrades };
            return new Response<object> { status = "success", message = "ok", data = new { Trades = ME_Gateway.Instance[pair].AllTrades } };
        }

        [HttpGet]
        [Route("~/Notifications")]
        public Response<object> Notifications(string pair)
        {
            if (string.IsNullOrWhiteSpace(pair))
                return new Response<object> { status = "badrequest", message = "invalid pair" };
            return new Response<object> { status = "success", message = "ok", data = new { Trades = ME_Gateway.Instance[pair].AllMatchResponses } };
        }

        [HttpGet]
        [Route("~/Stats")]
        public Response<object> TradeStats(string pair)
        {
            if (string.IsNullOrWhiteSpace(pair))
                return new Response<object> { status = "success", message = "ok", data = ME_Gateway.Instance.listedPairs.Select(_pair => new { pair = _pair, stat = ME_Gateway.Instance[_pair].GetStats }).ToList() };
            else
                return new Response<object> { status = "success", message = "ok", data = new { pair, stat = ME_Gateway.Instance[pair].GetStats } };
        }

        [HttpGet]
        [Route("~/ResetME")]
        public Response<object> ResetME(string pair,bool remove=true)
        {
            ME_Gateway.Instance.ResetPair(pair, remove);
            return new Response<object> { status = "success", message = "ok", data = "success" };
        }

    }
}
