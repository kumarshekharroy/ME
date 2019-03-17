using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.Utility
{
    static class Helper_Static
    {

        public static decimal TruncateDecimal(this decimal input,int precision=8)
        {
            var temp=(decimal)Math.Pow(10, precision);
            //return decimal.Truncate(input * temp) / temp;
            temp = decimal.Truncate(input * temp) / temp;
            return temp;

        }
    }
    class Helper
    {
    }
}
