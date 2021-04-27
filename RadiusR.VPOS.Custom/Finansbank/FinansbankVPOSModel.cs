using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom.Finansbank
{
    public class FinansbankVPOSModel : VPOSGenericModel
    {
        public string HostUrl { get { return "https://vpostest.qnbfinansbank.com/Gateway/Default.aspx"; } }
        public int MbrId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecureType { get { return "NonSecure"; } }
        public string TxnType { get { return "Auth"; } }
        public new int CurrencyCode { get { return 949; } }
        public override string ToString()
        {
            return string.Join("&", new string[] {
                $"MbrId={MbrId}",
                $"MerchantID={MerchantId}",
                $"UserCode={Username}",
                $"UserPass={Password}",
                $"SecureType={SecureType}",
                $"TxnType={TxnType}",
                $"PurchAmount={CurrencyAmount}",
                $"Currency={CurrencyCode}",
                $"Pan={CreditCardNo}",
                $"Expiry={ExpiryMonth}{ExpiryYear}",
                $"Cvv2={CVV}",
            });
        }
        public static ServiceResponse FinansPostForm(FinansbankVPOSModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new ByteArrayContent(Encoding.UTF8.GetBytes(model.ToString()));
                    var response = client.PostAsync(model.HostUrl, content).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return new ServiceResponse()
                        {
                            Message = response.Content.ReadAsStringAsync().Result,
                            Code = ((int)response.StatusCode).ToString(),
                            IsSuccess = true
                        };
                    }
                    else
                    {
                        return new ServiceResponse()
                        {
                            Message = response.Content.ReadAsStringAsync().Result,
                            Code = ((int)response.StatusCode).ToString(),
                            IsSuccess = false
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse()
                {
                    IsSuccess = false,
                    Code = ((int)HttpStatusCode.InternalServerError).ToString(),
                    Message = ex.Message
                };
            }
        }
    }
}
