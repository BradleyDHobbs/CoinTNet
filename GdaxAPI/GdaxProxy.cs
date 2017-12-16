using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Web;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using WebSocketSharp;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GdaxAPI
{
    /// <summary>
    /// Constant for error codes
    /// </summary>
    public class ErrorCodes
    {
        /// <summary>
        /// We don't handle that error 
        /// </summary>
        public const int UnknownError = 0;
        /// <summary>
        /// Invalid/empty API keys
        /// </summary>
        public const int InvalidAPIKeys = -1;
    }

    public class RootObject
    {
        public string type { get; set; }
        public List<string> product_ids { get; set; }
        public List<object> channels { get; set; }
    }

    public class Channel
    {
        public string name { get; set; }
        public List<string> product_ids { get; set; }
    }

    /// <summary>
    /// Proxy for making calls to the  Bitsamp API
    /// </summary>
    public class GdaxProxy
    {
        #region Private Members
        /// <summary>
        /// The Secret key
        /// </summary>
        private string _secretKey;
        /// <summary>
        /// The API Key
        /// </summary>
        private string _apiKey;
        /// <summary>
        /// The client ID
        /// </summary>
        private string _passphrase;
        /// <summary>
        /// The API's base URL
        /// </summary>
        private string _baseURL;
        /// <summary>
        /// The current fee
        /// </summary>
        private decimal _fee = 0.0M;
        /// <summary>
        /// Used to compute hmac signature to sign data sent to the private API
        /// </summary>
        private HMACSHA256 _hmac;
        /// <summary>
        /// Message displayed when there are no API keys
        /// </summary>
        private const string InvalidKeysErrMsg = "API key not found";
        /// <summary>
        /// Used to track current currency pair
        /// </summary>
        private string product_id { get; set; }

        private WebSocket _webSocket;
        private WebSocket webSocket { get; set; }
        private string webSocketData { get; set; }
        #endregion

        /// <summary>
        /// Initialises a new instance of the BitstampProxy class
        /// </summary>
        /// <param name="baseURL"></param>
        /// <param name="passphrase"></param>
        /// <param name="apiKey"></param>
        /// <param name="privateKey"></param>
        public GdaxProxy(string baseURL, string passphrase, string apiKey, string privateKey)
        {
            _passphrase = passphrase;
            _apiKey = apiKey;
            _secretKey = privateKey;
            _baseURL = baseURL;

            _hmac = new HMACSHA256(Convert.FromBase64String(_secretKey != null ? _secretKey : string.Empty));
        }

        /// <summary>
        /// Gets Bitstamp's ticker
        /// </summary>
        /// <returns></returns>
        public CallResult<Ticker> GetTicker()
        {
            if (string.IsNullOrWhiteSpace(webSocketData))
                return new CallResult<Ticker>();

            return new CallResult<Ticker>()
            {
                Result = Ticker.CreateFromString(webSocketData)
            };
        }

        public async Task GetWebSocketTicker(string pair)
        {
            try
            {
                if (webSocket != null && (webSocket.ReadyState == WebSocketState.Open || webSocket.ReadyState == WebSocketState.Connecting))
                    return;
                else
                {
                    product_id = pair;
                    var json = new RootObject()
                    {
                        type = "subscribe",
                        product_ids = new List<string>() {
                            pair
                        },
                        channels = new List<object>() {
                        "ticker",
                        new Channel() {
                            name = "ticker",
                            product_ids = new List<string>() {
                                pair
                            }
                        }
                    }
                    };
                    string stringToSend = JsonConvert.SerializeObject(json);

                    webSocket = new WebSocket("wss://ws-feed.gdax.com");
                    webSocket.OnMessage += (sender, e) => 
                    {
                        if (e.IsText)
                        {
                            // Do something with e.Data.
                            webSocketData = e.Data;
                        }

                        if (e.IsBinary)
                        {
                          // Do something with e.RawData.
                          
                        }
                    };
                            

                    webSocket.Connect();
                    webSocket.Send(stringToSend);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
        }

        public void CloseCurrentWebsocket()
        {
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
                webSocket.Close();
        }

        //public async Task Send(ClientWebSocket webSocket)
        //{
        //    while (webSocket.State == WebSocketState.Open)
        //    {
        //        Console.WriteLine("Sending stuff to server");
        //        var json = new RootObject {
        //            type = "subscribe",
        //            product_ids = {
        //                "BTC-USD"
        //            },
        //            channels = {
        //                "ticker",
        //                new {
        //                    name = "ticker",
        //                    product_ids = new List<string>() {
        //                        "BTC-USD"
        //                    }
        //                }
        //            }
        //        };
        //        string stringToSend = JsonConvert.SerializeObject(json);
        //        byte[] buffer = Encoding.UTF8.GetBytes(stringToSend);

        //        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, CancellationToken.None);

        //        Console.WriteLine("Sent: " + stringToSend);
        //        await Task.Delay(1000);
        //    }
        //}

        //public async Task Receive(ClientWebSocket webSocket)
        //{
        //    byte[] buffer = new byte[1024];
        //    while (webSocket.State == WebSocketState.Open)
        //    {
        //        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        if (result.MessageType == WebSocketMessageType.Close)
        //        {
        //            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        //        }
        //        else
        //        {
        //            Console.WriteLine("Receive: " + Encoding.UTF8.GetString(buffer).TrimEnd('\0'));
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the full order book
        /// </summary>
        /// <returns></returns>
        public CallResult<OrderBook> GetOrderBook()
        {
            return MakeGetRequest<OrderBook>($"/orders?status=all&product_id={product_id}", result => OrderBook.CreateFromJson(result)).Result;
        }

        /// <summary>
        /// Retrives all public transactions for the past minute/hour
        /// </summary>
        /// <param name="lastMinuteOnly">True if we want the transactions for the last minute only</param>
        /// <returns></returns>
        public CallResult<TransactionList> GetTransactions(bool lastMinuteOnly = false)
        {
            string url = "transactions/" + (lastMinuteOnly ? "?time=minute" : string.Empty);
            return MakeGetRequest(url, result => TransactionList.ReadFromJObject(result)).Result;
        }

        /// <summary>
        /// Gets the user's balance
        /// </summary>
        /// <returns></returns>
        public CallResult<Balance> GetBalance()
        {
            return MakePostRequest("accounts/", r =>
            {
                Balance balance = Balance.CreateFromJObject(r as JObject);
                _fee = balance != null ? balance.Fee : 0.0M;
                return balance;
            });
        }

        /// <summary>
        /// Places a buy order
        /// </summary>
        /// <param name="amount">The amount of BTCs to buy</param>
        /// <param name="price">The price  per BTC</param>
        /// <returns></returns>
        public CallResult<OrderDetails> PlaceBuyOrder(decimal amount, decimal price)
        {
            return MakePostRequest("buy/",
                r => OrderDetails.CreateFromJObject(r as JObject),

                new Dictionary<string, string> {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                    {"price",price.ToString(CultureInfo.InvariantCulture)}
                });
        }

        /// <summary>
        /// Places a sell order
        /// </summary>
        /// <param name="amount">The amount of BTCs to sell</param>
        /// <param name="price">The price per BTC</param>
        /// <returns></returns>
        public CallResult<OrderDetails> PlaceSellOrder(decimal amount, decimal price)
        {
            return MakePostRequest("sell/",
                r => OrderDetails.CreateFromJObject(r as JObject),

                new Dictionary<string, string> {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                    {"price",price.ToString(CultureInfo.InvariantCulture)}
                });
        }

        /// <summary>
        /// Cancels an open order
        /// </summary>
        /// <param name="orderId">The order's ID</param>
        /// <returns></returns>
        public CallResult<bool> CancelOrder(long orderId)
        {
            //var args = GetAuthenticationArgs();
            //args["id"] = orderId.ToString();
            //var resultStr = SendPostRequest("cancel_order/", args);
            //var result = resultStr == "true" ? new JObject() : JObject.Parse(resultStr);
            //return ParseCallResult<bool>(result, r => resultStr == "true");
            return new CallResult<bool>();
        }

        /// <summary>
        /// Gets the fee associated with the user's account
        /// </summary>
        /// <returns></returns>
        public CallResult<decimal> GetFee()
        {
            if (decimal.Compare(_fee, 0.0M) == 0)
            {
                GetBalance();
            }
            return new CallResult<decimal>(_fee);
        }
        /// <summary>
        /// Gets a list of all the user's open orders
        /// </summary>
        /// <returns></returns>
        public CallResult<OpenOrdersContainer> GetOpenOrders()
        {
            return MakePostRequest("open_orders/", t => OpenOrdersContainer.CreateFromJObject(t as JArray));
        }

        #region Private methods

        /// <summary>
        /// Builds post data for a request
        /// </summary>
        /// <param name="dataDic"></param>
        /// <returns></returns>
        private static string BuildPostData(Dictionary<string, string> dataDic)
        {
            var p = dataDic.Keys.Select(key => String.Format("{0}={1}", key, HttpUtility.UrlEncode(dataDic[key]))).ToArray();
            return string.Join("&", p);
        }

        /// <summary>
        /// Parses the result of a call to the API and converts that result into an object, wrapped in a CallResult object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private CallResult<T> ParseCallResult<T>(JToken token, Func<JToken, T> func)
        {
            JToken val;
            string error = null;
            var result = token as JObject;

            if (result != null && result.TryGetValue("error", out val))
            {
                var jValue = val as JValue;
                if (jValue != null)
                {
                    error = (string)jValue.Value;
                }
                else
                {
                    error = string.Join("\n", val["__all__"].Select(jt => ((JValue)jt).Value));
                }
            }

            var r = new CallResult<T>
            {
                ErrorMessage = error,
                ErrorCode = error == InvalidKeysErrMsg ? ErrorCodes.InvalidAPIKeys : ErrorCodes.UnknownError,
                Result = string.IsNullOrEmpty(error) ? func(token) : default(T)
            };
            return r;
        }

        /// <summary>
        /// Sends a post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private string SendPostRequest(string url, Dictionary<string, string> args)
        {
            url = _baseURL + url;
            var dataStr = BuildPostData(args);
            var data = Encoding.ASCII.GetBytes(dataStr);
            var request = WebRequest.Create(new Uri(url));

            request.Method = "POST";
            request.Timeout = 15000;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            var response = request.GetResponse();
            using (var resStream = response.GetResponseStream())
            {
                using (var resStreamReader = new StreamReader(resStream))
                {
                    string resString = resStreamReader.ReadToEnd();
                    return resString;
                }
            }

        }

        /// <summary>
        /// Sends a GET Request
        /// </summary>
        /// <param name="url">The relative url to which the request will be sent</param>
        /// <returns>The request's result</returns>
        private async Task<string> SendGETRequest(string url)
        {
            //url = _baseURL + url;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseURL);
                client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("User-Agent", "GDAXClient");
                foreach (KeyValuePair<string, string> entry in GetAuthenticationArgs("GET", url))
                {
                    client.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Makes a GET request and returns the response as an object wrapped in a CallResult
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="url">The relative URL to make the call to</param>
        /// <param name="conversion">The function used to convert the response into an object</param>
        /// <returns></returns>
        private async Task<CallResult<T>> MakeGetRequest<T>(string url, Func<string, T> conversion)
        {
            try
            {
                var result = await SendGETRequest(url);
                //var result = JToken.Parse(resultStr);
                return ParseCallResult(result, r => conversion(result));
            }
            catch (Exception e)
            {
                return new CallResult<T> { ErrorMessage = e.Message, Exception = e };
            }
        }

        /// <summary>
        /// Makes a Post request and returns the response as an object wrapped in a CallResult
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="url">The relative URL to make the call to</param>
        /// <param name="conversion">The function used to convert the response into an object</param>
        /// <param name="extraArgs">The extra parameters to pass to the POST request</param>
        /// <returns></returns>
        private CallResult<T> MakePostRequest<T>(string url, Func<JToken, T> conversion, Dictionary<string, string> extraArgs = null)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey) || string.IsNullOrEmpty(_passphrase))
            {
                return new CallResult<T> { ErrorMessage = "Missing API Keys/Client ID", ErrorCode = ErrorCodes.InvalidAPIKeys };
            }
            try
            {
                var args = GetAuthenticationArgs("POST", url);
                if (extraArgs != null)
                {
                    foreach (var kvp in extraArgs)
                    {
                        args[kvp.Key] = kvp.Value;
                    }
                }
                var resultStr = SendPostRequest(url, args);
                var result = JToken.Parse(resultStr);
                return ParseCallResult(result, r => conversion(result));
            }
            catch (Exception e)
            {
                return new CallResult<T> { ErrorMessage = e.Message, Exception = e };
            }
        }

        /// <summary>
        /// Returns a dictionary containing the parameters required for Gdax authentication
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetAuthenticationArgs(string type, string path)
        {
            string unixTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string body = string.Empty;
            string message = unixTime + type + path + body;
            var signature = Convert.ToBase64String(_hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
            
            var args = new Dictionary<string, string>()
            {
                { "CB-ACCESS-KEY", _apiKey },
                { "CB-ACCESS-SIGN", signature },
                { "CB-ACCESS-TIMESTAMP", unixTime },
                { "CB-ACCESS-PASSPHRASE", _passphrase }
            };

            return args;
        }
        #endregion
    }
}
