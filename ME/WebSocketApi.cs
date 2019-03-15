using System.Collections.Concurrent;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ME
{
    class WebSocketApi
    {
        private static ConcurrentBag<string> connectionIDS = new ConcurrentBag<string>();
        public class Laputa : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                var msg = e.Data == "BALUS"
                          ? "I've been balused already..."
                          : "I'm not available now.";

                Send(msg);
            }
            protected override void OnOpen()
            {
                
                base.OnOpen();
            }

            protected override void OnClose(CloseEventArgs e)
            {
                base.OnClose(e);
            }
            protected override void OnError(ErrorEventArgs e)
            {
                base.OnError(e);
            }
        }
    }
}
