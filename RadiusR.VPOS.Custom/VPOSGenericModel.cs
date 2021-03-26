using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom
{
    public class VPOSGenericModel
    {
        public string MerchantId { get; set; }
        public string TerminalNo { get; set; }
        public string Password { get; set; }
        public decimal CurrencyAmount { get; set; }
        public int CurrencyCode { get; set; }
        public string BillingCustomerName { get; set; }
    }
}
