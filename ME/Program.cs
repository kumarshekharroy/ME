﻿using ME.Utility;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace ME
{
    class Program
    {
        public static WebSocketServer wssv;
        public static void Main(string[] args)
        {
            //Task.Factory.StartNew(()=>StartWCServer());
            var url = "http://localhost:8080";
            using (WebApp.Start<Startup>(url: url))
            {
                Console.WriteLine($"Web Server is running at : {url}.");

               var MeInstance= ME_Gateway.Instance;
                StartWCServer();
                //Me_Client.PlaceAllOrder(Me_Client.getRandomOrders(200000), true,false);

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }

           
        }
         public static void StartWCServer()
        {
             wssv = new WebSocketServer("ws://localhost:8088");
            wssv.AddWebSocketService<WC_MatchTicker>("/MEWC_MatchTicker");
            wssv.AddWebSocketService<WC_TradeTicker>("/MEWC_TradeTicker");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
         
        #region permutation
        public static int serial_no = 0;
        static void CalcPermutation()
        {
            string str = "SHEKHARKUMARROY";
            char[] charArry = str.ToCharArray();
            permute(charArry, 0, charArry.Count() - 1); 
        }

        static void permute(char[] arry, int i, int n)
        {
            if (i == n)
            {
                serial_no++;
                Console.WriteLine(serial_no + "=>" + new string(arry));
            }
            else
            {
                for (int j = i; j <= n; j++)
                {
                    swap(ref arry[i], ref arry[j]);
                    permute(arry, i + 1, n);
                    swap(ref arry[i], ref arry[j]); //backtrack
                }
            }
        }

        static void swap(ref char a, ref char b)
        {
            char tmp = a;
            a = b;
            b = tmp;
        }
        #endregion


         
         
    }
}
