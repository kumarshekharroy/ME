using Newtonsoft.Json; 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq; 
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ME
{

    public class WebsocketUsersDetail
    {
        public string ConnectionId { get; set; } 
        public DateTime SessionStart { get; set; } 
    }


    public class WC_MatchTicker : WebSocketBehavior
    {
        private static ConcurrentDictionary<string, List<WebsocketUsersDetail>> Pair_connectionIDS = new ConcurrentDictionary<string, List<WebsocketUsersDetail>>();

        private static ConcurrentDictionary<string, string> ConnIDToPairMap = new ConcurrentDictionary<string, string>();


        public static WC_MatchTicker Instance = null;

        public WC_MatchTicker()
        {
            Instance = this;
        }
        public static void PushTicker(string pair, object data)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(data); 

                List<WebsocketUsersDetail> websocketUsersDetail; 
                if (Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
                    websocketUsersDetail.ForEach(userConnDetail =>
                    {
                        Program.wssv.WebSocketServices["/MEWC_MatchTicker"].Sessions.SendToAsync(payload, userConnDetail.ConnectionId, (isSent) =>
                        {
                            //if (!isSent) throw new MissingMemberException();
                        });
                    });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception => {ex.Message}");
            }

        }

        protected override void OnOpen()
        {
            Send("Status : Connecting"); 
            var pair = string.IsNullOrWhiteSpace(Context.QueryString["pair"]) ? "All": Context.QueryString["pair"];

            if (!ConnIDToPairMap.TryAdd(ID, pair))
                return;

            List<WebsocketUsersDetail> websocketUsersDetail;

            if (Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
                websocketUsersDetail.Add(new WebsocketUsersDetail { ConnectionId = ID,  SessionStart = DateTime.UtcNow });
            else
                Pair_connectionIDS.TryAdd(pair, new List<WebsocketUsersDetail> { new WebsocketUsersDetail { ConnectionId = ID, SessionStart = DateTime.UtcNow } }); 
            Send("Status : Connected");

            if (pair != "All")
            {
                Send("Init :" + JsonConvert.SerializeObject(new { Buy = ME_Gateway.Instance[pair].OrderBookBuy , Sell= ME_Gateway.Instance[pair].OrderBookSell }));
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            string pair;
            if (!ConnIDToPairMap.TryGetValue(ID, out pair))
                return;

            List<WebsocketUsersDetail> websocketUsersDetail;

            if(Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
            {
                var userConnDetail = websocketUsersDetail.Where(user => user.ConnectionId == ID).FirstOrDefault();
                if (userConnDetail == null)
                    return;
                websocketUsersDetail.Remove(userConnDetail);
            } 
        }
        protected override void OnError(ErrorEventArgs e)
        {
           // Send(" Status: Error : " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Send("Status : Live");
            
        }
    }

    public class WC_TradeTicker : WebSocketBehavior
    {
        private static ConcurrentDictionary<string, List<WebsocketUsersDetail>> Pair_connectionIDS = new ConcurrentDictionary<string, List<WebsocketUsersDetail>>();


        private static ConcurrentDictionary<string, string> ConnIDToPairMap = new ConcurrentDictionary<string, string>();

        public static WC_TradeTicker Instance = null;

        public WC_TradeTicker()
        {
            Instance = this;
        }
        public static void PushTicker(string pair, object data)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(data); 
                List<WebsocketUsersDetail> websocketUsersDetail;
                if (Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
                    websocketUsersDetail.ForEach(userConnDetail =>
                    {
                        Program.wssv.WebSocketServices["/MEWC_TradeTicker"].Sessions.SendToAsync(payload, userConnDetail.ConnectionId, (isSent) =>
                        {
                            //if (!isSent) throw new MissingMemberException();
                        });
                    });
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Exception => {ex.Message}");
            }

        }

        protected override void OnOpen()
        {
            Send("Status : Connecting");
            var pair = string.IsNullOrWhiteSpace(Context.QueryString["pair"]) ? "All" : Context.QueryString["pair"];

            if (!ConnIDToPairMap.TryAdd(ID, pair))
                return;

            List<WebsocketUsersDetail> websocketUsersDetail;
            if (Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
                websocketUsersDetail.Add(new WebsocketUsersDetail { ConnectionId = ID,   SessionStart = DateTime.UtcNow });
            else
                Pair_connectionIDS.TryAdd(pair, new List<WebsocketUsersDetail> { new WebsocketUsersDetail { ConnectionId = ID,  SessionStart = DateTime.UtcNow } });

            Send("Status : Connected");

            if(pair!="All")
            {
                Send("Init :"+ JsonConvert.SerializeObject(ME_Gateway.Instance[pair].AllTrades.Reverse().Take(50)));
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            string pair;
            if (!ConnIDToPairMap.TryGetValue(ID, out pair))
                return;

            List<WebsocketUsersDetail> websocketUsersDetail;

            if (Pair_connectionIDS.TryGetValue(pair, out websocketUsersDetail))
            {
                var userConnDetail = websocketUsersDetail.Where(user => user.ConnectionId == ID).FirstOrDefault();
                if (userConnDetail == null)
                    return;
                websocketUsersDetail.Remove(userConnDetail);
            }
        }
        protected override void OnError(ErrorEventArgs e)
        {
            //Send(" Status: Error : " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Send("Status : Live");
        }
    }

}
