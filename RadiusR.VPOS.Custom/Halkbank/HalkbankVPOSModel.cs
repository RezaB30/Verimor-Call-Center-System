using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom.Halkbank
{
    public class HalkbankVPOSModel : VPOSGenericModel
    {
        public string Host { get { return "https://sanalpos.halkbank.com.tr/fim/api"; } }
        public string ChargeType { get { return "Auth"; } }
        public new int CurrencyCode { get { return 949; } }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
