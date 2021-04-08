using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom.Vakifbank
{
    public class VakifbankVPOSModel : VPOSGenericModel
    {
        public string TransactionType { get { return "Sale"; } }
        public string ClientIP { get; set; }
        public int TransactionDeviceSource { get { return 0; } }
        public string Expiry { get { return "20" + ExpiryYear + ExpiryMonth; } }
        public new int CurrencyCode { get { return 949; } }
    }
}
