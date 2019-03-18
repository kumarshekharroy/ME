﻿using ME.Utility;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ME
{
    class Program
    { 
        public static void Main(string[] args)
        {
            //Task.Factory.StartNew(MainService.Instance.MatchMyOrder_CornJob);
            var url = "http://localhost:8080";
            using (WebApp.Start<Startup>(url: url))
            {
                Console.WriteLine($"Web Server is running at : {url}.");

                Me_Client.PlaceAllOrder(Me_Client.getRandomOrders(200000), true,false);
                 
               // Console.WriteLine(JsonConvert.SerializeObject(MainService.Instance.GetStats, Formatting.Indented));

                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }

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
