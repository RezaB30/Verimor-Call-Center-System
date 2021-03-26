using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.VPOS.Custom
{
    public class VPOSPaymentClient
    {
        private string _MerchantId { get; set; }
        private string _StoreKey { get; set; }
        private string _MerchantSalt { get; set; }
        private int _VPOSTypeId { get; set; }
        public VPOSPaymentClient(int VPOSTypeId, string merchantId, string storeKey, string merchantSalt)
        {
            _MerchantId = merchantId;
            _StoreKey = storeKey;
            _MerchantSalt = merchantSalt;
            _VPOSTypeId = VPOSTypeId;
        }
        public ServiceResponse Payment(decimal currencyAmount, string customerName, string description, string clientIp, string creditCardNo, string cvv, string expiryMonth, string expiryYear)
        {
            var VPOSType = (RadiusR.DB.Enums.VPOSTypes)_VPOSTypeId;
            switch (VPOSType)
            {
                case RadiusR.DB.Enums.VPOSTypes.Vakif:
                    {
                        var model = new Vakifbank.VakifbankVPOSModel()
                        {
                            ClientIP = clientIp,
                            CurrencyAmount = currencyAmount,
                            CurrencyCode = 949,
                            Expiry = expiryYear + expiryMonth,
                            MerchantId = _MerchantId,
                            BillingCustomerName = customerName,
                            PAN = creditCardNo,
                            Password = _StoreKey,
                            TerminalNo = _MerchantSalt,
                            CVV = cvv,
                        };
                        return _VakifBankVPOS(model);
                    }
                default:
                    {
                        return new ServiceResponse()
                        {
                            Code = "200",
                            Message = "System VPOS Not Found",
                            IsSuccess = false
                        };
                    }
            }

        }
        private ServiceResponse _VakifBankVPOS(Vakifbank.VakifbankVPOSModel model)
        {
            VakifbankVPOSReference.TransactionWebServicesSoapClient client = new VakifbankVPOSReference.TransactionWebServicesSoapClient();
            var response = client.ExecuteVposRequest(new VakifbankVPOSReference.VposRequest()
            {
                TransactionType = model.TransactionType,
                ClientIp = model.ClientIP,
                TerminalNo = model.TerminalNo,
                Password = model.Password,
                OrderDescription = model.BillingCustomerName,
                CurrencyAmount = model.CurrencyAmount,
                CurrencyCode = model.CurrencyCode,
                TransactionDeviceSource = model.TransactionDeviceSource,
                CustomerName = model.BillingCustomerName,
                Expiry = model.Expiry,
                Cvv = model.CVV,
                MerchantId = model.MerchantId
            });
            return new ServiceResponse()
            {
                Code = response.ResultCode,
                Message = response.ResultDetail,
                IsSuccess = response.ResultCode == "0000" ? true : false
            };
        }

    }
}
