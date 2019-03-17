
using ME.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http; 

namespace ME
{
   public class OrderController:ApiController
    {

        //[HttpGet]
        //[Route("GetAA")]
        //public Response<object> Get()
        //{

        //    return new Response<object> { status = "success", message = "ok", data = "" };
        //}

        [HttpPost]
        [Route("~/PlaceOrder")]
        public Response<object> PlaceOrder(Order order)
        {
            if (order == null)
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = MainService.Instance.PlaceMyOrder(order);

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
            var Response = MainService.Instance.PlaceMyOrder(order);

            // Console.WriteLine(JsonConvert.SerializeObject(Response, Formatting.Indented));
            Console.WriteLine($"\t \t \t \t {Response.ID}");
            return new Response<object> { status = "success", message = "order processed successfully", data = Response };
        }
        [HttpPost]
        [Route("~/PlaceBulkOrders")]
        public Response<object> PlaceBulkOrders(List<Order> orders)
        {
            if (orders == null || orders.Count==0)
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = MainService.Instance.PlaceMyBulkOrder(orders);

            // Console.WriteLine(JsonConvert.SerializeObject(Response, Formatting.Indented));
            //Console.WriteLine($"\t \t \t \t {Response.ID}");
            return new Response<object> { status = "success", message = "order processed successfully", data = Response };
        }
        [HttpGet]
        [Route("~/GetOrders/{Side=0}")]
        public Response<object> GetOrders(int side)
        {
            if (side == 1)
                return new Response<object> { status = "success", message = MainService.Instance.AllBuyOrders.Count.ToString(), data = new { BuyOrders = MainService.Instance.AllBuyOrders } };
            else if (side == 2)
                return new Response<object> { status = "success", message = MainService.Instance.AllSellOrders.Count.ToString(), data = new { SellOrders = MainService.Instance.AllSellOrders } };
            else
                return new Response<object> { status = "success", message =(MainService.Instance.AllBuyOrders.Count+MainService.Instance.AllSellOrders.Count).ToString(), data = new { BuyOrders = MainService.Instance.AllBuyOrders, SellOrders = MainService.Instance.AllSellOrders } };
        }

        [HttpGet]
        [Route("~/GetTrades")]
        public object GetTrades()
        {
            //return new { Trades = MainService.Instance.AllTrades };
            return new Response<object> { status = "success", message = "ok", data = new { Trades = MainService.Instance.AllTrades } };
        }

        [HttpGet]
        [Route("~/Notifications")]
        public Response<object> Notifications()
        {
            return new Response<object> { status = "success", message = "ok", data = new { Trades = MainService.Instance.AllMatchResponses } };
        }
        [HttpGet]
        [Route("~/Stats")]
        public Response<object> TradeStats()
        {  
            return new Response<object> { status = "success", message = "ok", data = MainService.Instance.GetStats };
        }
    }
}
