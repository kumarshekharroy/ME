using ME.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ME
{

    public class WebsocketUsersDetail
    {
        public string ConnectionId { get; set; }
        public bool ConnectionStatus { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
    }


    public class WCTicker : WebSocketBehavior
    {
        private static ConcurrentDictionary<string, List<WebsocketUsersDetail>> Pair_connectionIDS = new ConcurrentDictionary<string, List<WebsocketUsersDetail>>();

        //private static readonly Lazy<WCTicker> lazy = new Lazy<WCTicker>(() => new WCTicker());
        //private WCTicker()
        //{ 
        //}
        //public static WCTicker Instance { get { return lazy.Value; } }


        // private static WCTicker instance = null;
        // private static readonly object padlock = new object();

        //public WCTicker()
        // {
        // }

        // public static WCTicker Instance
        // {
        //     get
        //     {
        //         if (instance == null)
        //         {
        //             lock (padlock)
        //             {
        //                 if (instance == null)
        //                 {
        //                     instance = new WCTicker();
        //                 }
        //             }
        //         }
        //         return instance;
        //     }
        // }









        public static volatile WCTicker Instance = null;

        public WCTicker()
        {
            Instance = this;
        }
        public void  PushTicker(string pair, object data)
        {
            try
            {
                Sessions.Broadcast("kjscs");
                var payload = JsonConvert.SerializeObject(data);
                Parallel.ForEach(Pair_connectionIDS.Keys.Where(key => key.ToLower() == pair.ToLower() || key.ToLower() == "all"), (key) =>
                {
                    Parallel.ForEach(Pair_connectionIDS[key].Where(userConnDetail => userConnDetail.ConnectionStatus), (userConnDetail) =>
                    {
                        Sessions.SendToAsync(payload, userConnDetail.ConnectionId, (isSent) => { if (!isSent) throw new MissingMemberException(); });
                    });
                }); 

            }
            catch (Exception ex)
            {


            }

        }

        protected override void OnOpen()
        {
            Send(JsonConvert.SerializeObject(new Response<object> { status = "success", data = "connecting." }));
            var pair = Context.QueryString["pair"] ?? "All";
            List<WebsocketUsersDetail> websocketUsersDetail;
            if(Pair_connectionIDS.TryGetValue(pair,out websocketUsersDetail))
                websocketUsersDetail.Add(new WebsocketUsersDetail { ConnectionId = ID, ConnectionStatus = true, SessionStart = DateTime.UtcNow });
            else
                Pair_connectionIDS.TryAdd(pair,new List<WebsocketUsersDetail> { new WebsocketUsersDetail { ConnectionId = ID, ConnectionStatus = true, SessionStart = DateTime.UtcNow } }); 

            Send(JsonConvert.SerializeObject(new Response<object> { status = "success", data = "connected." }));
            base.OnOpen();
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Parallel.ForEach(Pair_connectionIDS.Values, users =>
             {
                 var userConnDetail = users.Where(user => user.ConnectionId == ID).FirstOrDefault();
                 if (userConnDetail == null)
                     return;
                 userConnDetail.ConnectionStatus = false;
                 userConnDetail.SessionEnd = DateTime.UtcNow;
                 Send(JsonConvert.SerializeObject(new Response<object> { status = "error", message = "session closed." }));
             });
        }
        protected override void OnError(ErrorEventArgs e)
        {
            Send("error : " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Send("alive");
        }
    }

}
