using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{
    class Order
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public int Side { get; set; }
        public int Type { get; set; }
        public decimal Volume { get; set; }
        public decimal Rate { get; set; }
        public decimal PendingVolume { get; set; }
        public int Status { get; set; }  
    }
}
