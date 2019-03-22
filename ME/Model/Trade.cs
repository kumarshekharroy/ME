using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ME.Model
{
    public class Trade
    {
        public Trade(DateTime? dateTime)
        {
            TimeStamp = dateTime ?? DateTime.UtcNow;
        }
        public long ID { get; set; }
        public string Pair { get; set; }
        public long UserID_Buyer { get; set; }
        public long UserID_Seller { get; set; }
        public long OrderID_Buy { get; set; }
        public long OrderID_Sell { get; set; }
        public OrderSide Side { get; set; }
        public decimal Volume { get; set; }
        public decimal Rate { get; set; }
        public DateTime TimeStamp { get; set; }
    } 
    public class Custom_ConcurrentQueue<T> : ConcurrentQueue<T>
    {
        public delegate void ItemAddedDelegate(T item);
        public event ItemAddedDelegate ItemAdded;

        public new virtual void Enqueue(T item)
        {
            base.Enqueue(item);
            if (ItemAdded != null)
            {
                ItemAdded(item);
            }
        } 

    }
}
