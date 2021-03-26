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
        public string PAN { get; set; }
        public string Expiry { get; set; } // YYYYMM
        public string CVV { get; set; }
        public string ClientIP { get; set; }
        public int TransactionDeviceSource { get { return 0; } }
    }
}
