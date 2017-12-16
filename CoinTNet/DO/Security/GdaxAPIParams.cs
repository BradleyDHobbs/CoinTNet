using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinTNet.DO.Security
{
    public class GdaxAPIParams
    {
        /// <summary>
        /// Gets or sets the API key
        /// </summary>
        public string APIKey { get; set; }
        /// <summary>
        /// Gets or sets the secret
        /// </summary>
        public string APISecret { get; set; }
        /// <summary>
        /// Gets or sets the client ID
        /// </summary>
        public string Passphrase { get; set; }
    }
}
