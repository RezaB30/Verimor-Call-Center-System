using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom
{
    public class VPOSPaymentRequest
    {
        public decimal CurrencyAmount { get; set; }
        public string CustomerName { get; set; }
        public string Description { get; set; }
        public string ClientIP { get; set; }
        public string CreditCardNo { get; set; }
        public string CVV { get; set; }
        public string ExpiryYear { get; set; }
        public string ExpiryMonth { get; set; }

    }
}
