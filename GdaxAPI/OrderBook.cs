using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GdaxAPI
{
    /// <summary>
    /// Represents the order book
    /// </summary>
    public class OrderBook
    {
        /// <summary>
        /// The list of sell orders
        /// </summary>
        public List<SimpleOrderInfo> Orders { get; private set; }
        /// <summary>
        /// The list of buy orders
        /// </summary>
        public List<SimpleOrderInfo> Bids { get; private set; }

        public static OrderBook CreateFromJson(string json)
        {
            var r = new OrderBook();
            r.Orders = new List<SimpleOrderInfo>();
            r.Bids = new List<SimpleOrderInfo>();

            r.Orders = JsonConvert.DeserializeObject<List<SimpleOrderInfo>>(json);

            //foreach (var item in o as JArray)
            //{
            //    var order = SimpleOrderInfo.CreateFromJObject(item as JArray);
            //    r.Asks.Add(order);
            //}
            //foreach (var item in o["bids"] as JArray)
            //{
            //    var order = SimpleOrderInfo.CreateFromJObject(item as JArray);
            //    r.Bids.Add(order);
            //}

            return r;
        }

    }

    /// <summary>
    /// Represents an order from the order book
    /// </summary>
    public class SimpleOrderInfo
    {
        public string id { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string product_id { get; set; }
        public string side { get; set; }
        public string stp { get; set; }
        public string type { get; set; }
        public string time_in_force { get; set; }
        public bool post_only { get; set; }
        public DateTime created_at { get; set; }
        public string fill_fees { get; set; }
        public string filled_size { get; set; }
        public string executed_value { get; set; }
        public string status { get; set; }
        public bool settled { get; set; }

        //public static SimpleOrderInfo CreateFromJObject(JArray obj)
        //{
        //    if (obj == null)
        //    {
        //        return null;
        //    }

        //    var r = new SimpleOrderInfo()
        //    {
        //        Price = obj.Value<decimal>(0),
        //        Amount = obj.Value<decimal>(1),
        //    };

        //    return r;
        //}

    }
}
