using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CoinTNet.DO.Security;
using CoinTNet.Common.Constants;
using CoinTNet.DAL;
using CoinTNet.UI.Common.EventAggregator;
using CoinTNet.UI.Common;

namespace CoinTNet.UI.Controls.Options
{
    public partial class GdaxKeysControl : UserControl, Interfaces.IOptionControl
    {

        /// <summary>
        /// The previous key values
        /// </summary>
        private GdaxAPIParams _apiParams;
        /// <summary>
        /// Initialises a new instance of the class
        /// </summary>
        public GdaxKeysControl()
        {
            InitializeComponent();
            _apiParams = SecureStorage.GetEncryptedData<GdaxAPIParams>(SecuredDataKeys.GdaxAPI);
            txtPassphrase.Text = _apiParams.Passphrase;
            txtKey.Text = _apiParams.APIKey;
            txtSecret.Text = _apiParams.APISecret;
        }

        /// <summary>
        /// Saves the new keys
        /// </summary>
        /// <returns>True if the data was saved correctly</returns>
        public bool Save()
        {
            if (txtSecret.Text != _apiParams.APISecret || txtKey.Text != _apiParams.APIKey || txtPassphrase.Text != _apiParams.Passphrase)
            {
                GdaxAPIParams p = new GdaxAPIParams
                {
                    APIKey = txtKey.Text,
                    APISecret = txtSecret.Text,
                    Passphrase = txtPassphrase.Text
                };
                SecureStorage.SaveEncryptedData(p, SecuredDataKeys.BitstampAPI);
                ExchangeProxyFactory.NotifySettingsChanged(ExchangesInternalCodes.Bitstamp);
                EventAggregator.Instance.Publish(new SecuredDataChanged { DataKey = ExchangesInternalCodes.Gdax });
            }
            return true;

        }
    }
}
