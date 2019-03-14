
using ME.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;

namespace ME
{
    class OrderController:ApiController
    {

        [Route("~/PlaceOrder")]
        public Response<object> PlaceOrder(Order order)
        {
            if (order == null)
                return new Response<object> { status = "badrequest", message = "invalid order payload" };
            var Response = MainService.Instance.PlaceMyOrder(order);

            return new Response<object> { status = "success", message = "order processed successfully",data=Response };


        }
    }
}
