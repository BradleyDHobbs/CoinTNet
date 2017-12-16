using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GdaxAPI
{
    public class Ticker
    {
        /// <summary>
        /// The highest buy order
        /// </summary>
        public decimal Bid { get; private set; }
        /// <summary>
        /// The lowest sell order
        /// </summary>
        public decimal Ask { get; private set; }
        /// <summary>
        /// The 24 Hour volume
        /// </summary>
        public decimal Volume { get; private set; }
        /// <summary>
        /// The 24 h high
        /// </summary>
        public decimal High { get; private set; }
        /// <summary>
        /// The 24 H low
        /// </summary>
        public decimal Low { get; private set; }
        /// <summary>
        /// The price for the last order
        /// </summary>
        public decimal Last { get; private set; }


        public static Ticker CreateFromJObject(JObject o)
        {
            if (o == null)
            {
                return null;
            }

            var tick = new Ticker()
            {
                Bid = o.Value<decimal>("best_bid"),
                Ask = o.Value<decimal>("best_ask"),
                Volume = o.Value<decimal>("volume_24h"),
                High = o.Value<decimal>("high_24h"),
                Low = o.Value<decimal>("low_24h"),
                Last = o.Value<decimal>("price")
            };

            return tick;
        }

        public static Ticker CreateFromString(string json)
        {
            var data = JsonConvert.DeserializeObject<GdaxTickerJson>(json);
            var tick = new Ticker()
            {
                Bid = Convert.ToDecimal(data.best_bid),
                Ask = Convert.ToDecimal(data.best_ask),
                Volume = Convert.ToDecimal(data.volume_24h),
                High = Convert.ToDecimal(data.high_24h),
                Low = Convert.ToDecimal(data.low_24h),
                Last = Convert.ToDecimal(data.price)
            };

            return tick;
        }
    }

    public class GdaxTickerJson
    {
        public string type { get; set; }
        public long sequence { get; set; }
        public string product_id { get; set; }
        public string price { get; set; }
        public string open_24h { get; set; }
        public string volume_24h { get; set; }
        public string low_24h { get; set; }
        public string high_24h { get; set; }
        public string volume_30d { get; set; }
        public string best_bid { get; set; }
        public string best_ask { get; set; }
        public string side { get; set; }
        public DateTime time { get; set; }
        public int trade_id { get; set; }
        public string last_size { get; set; }
    }
}
