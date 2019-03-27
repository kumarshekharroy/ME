using System;
using System.Collections.Generic;
using System.Text;

namespace ME.Model
{
   public class Order :ICloneable
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public string Pair { get; set; }
        public OrderSide Side { get; set; }
        public OrderType Type { get; set; }
        public decimal Volume { get; set; }
        public decimal Rate { get; set; }
        public decimal Stop { get; set; }
        public bool IsStopActivated { get; set; }
        public decimal PendingVolume { get; set; }
        public DateTime AcceptedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public OrderStatus Status { get; set; }
        public OrderTimeInForce TimeInForce { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class BulkOrder
    {
        public List<Order> orders { get; set; }
        public string Pair { get; set; }
    }
}
