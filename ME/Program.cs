using Microsoft.Owin.Hosting;
using System;
using System.Linq; 

namespace ME
{
    class Program
    {


        public static int serial_no = 0;
        public static void Main(string[] args)
        {

            using (WebApp.Start<Startup>(url: "http://localhost:8088"))
            {
                Console.WriteLine("Web Server is running.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }










        }


















        #region permutation
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
