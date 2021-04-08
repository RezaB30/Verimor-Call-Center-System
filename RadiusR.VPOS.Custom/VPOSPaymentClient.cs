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
        private string _UserID { get; set; }
        private string _UserPass { get; set; }
        public VPOSPaymentClient(int VPOSTypeId, string merchantId, string storeKey, string merchantSalt, string UserID, string UserPass)
        {
            _MerchantId = merchantId;
            _StoreKey = storeKey;
            _MerchantSalt = merchantSalt;
            _VPOSTypeId = VPOSTypeId;
            _UserID = UserID;
            _UserPass = UserPass;
        }
        public ServiceResponse Payment(VPOSPaymentRequest request)
        {
            var VPOSType = (RadiusR.DB.Enums.VPOSTypes)_VPOSTypeId;
            switch (VPOSType)
            {
                case RadiusR.DB.Enums.VPOSTypes.Vakif:
                    {
                        var model = new Vakifbank.VakifbankVPOSModel()
                        {
                            ClientIP = request.ClientIP,
                            CurrencyAmount = request.CurrencyAmount,
                            ExpiryYear = request.ExpiryYear,
                            ExpiryMonth = request.ExpiryMonth,
                            MerchantId = _MerchantId,
                            BillingCustomerName = request.CustomerName,
                            CreditCardNo = request.CreditCardNo,
                            StoreKey = _StoreKey,
                            TerminalNo = _MerchantSalt,
                            CVV = request.CVV,
                        };
                        return _VakifBankVPOS(model);
                    }
                case RadiusR.DB.Enums.VPOSTypes.Halk:
                    {
                        var model = new Halkbank.HalkbankVPOSModel()
                        {
                            BillingCustomerName = request.CustomerName,
                            CreditCardNo = request.CreditCardNo,
                            CurrencyAmount = request.CurrencyAmount,
                            CVV = request.CVV,
                            ExpiryMonth = request.ExpiryMonth,
                            ExpiryYear = request.ExpiryYear,
                            TerminalNo = _MerchantSalt,
                            StoreKey = _StoreKey,
                            Password = _UserPass,
                            MerchantId = _MerchantId,
                            Username = _UserID
                        };
                        return _HalkBankVPOS(model);
                    }
                case DB.Enums.VPOSTypes.QNBFinans:
                    {
                        var model = new Finansbank.FinansbankVPOSModel()
                        {
                            BillingCustomerName = request.CustomerName,
                            CreditCardNo = request.CreditCardNo,
                            CurrencyAmount = request.CurrencyAmount,
                            CVV = request.CVV,
                            ExpiryMonth = request.ExpiryMonth,
                            ExpiryYear = request.ExpiryYear,
                            TerminalNo = _MerchantSalt,
                            StoreKey = _StoreKey,
                            Password = _UserPass,
                            MerchantId = _MerchantId,
                            Username = _UserID,
                            MbrId = 0 // will be test
                        };
                        return _FinansVPOS(model);
                    }
                case DB.Enums.VPOSTypes.Ziraat:
                    {
                        var model = new Ziraatbank.ZiraatVPOSModel()
                        {
                            CurrencyAmount = request.CurrencyAmount,
                            ExpiryYear = request.ExpiryYear,
                            ExpiryMonth = request.ExpiryMonth,
                            MerchantId = _MerchantId,
                            BillingCustomerName = request.CustomerName,
                            CreditCardNo = request.CreditCardNo,
                            StoreKey = _StoreKey,
                            TerminalNo = _MerchantSalt,
                            CVV = request.CVV,
                        };
                        return _ZiraatVPOS(model);
                    }
                case DB.Enums.VPOSTypes.PayTR:
                    {
                        var model = new PayTR.PayTRVPOSModel()
                        {
                            CurrencyAmount = request.CurrencyAmount,
                            ExpiryYear = request.ExpiryYear,
                            ExpiryMonth = request.ExpiryMonth,
                            MerchantId = _MerchantId,
                            BillingCustomerName = request.CustomerName,
                            CreditCardNo = request.CreditCardNo,
                            StoreKey = _StoreKey,
                            TerminalNo = _MerchantSalt,
                            CVV = request.CVV,
                        };
                        return _PayTRVPOS(model);
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
                Password = model.StoreKey,
                OrderDescription = model.BillingCustomerName,
                CurrencyAmount = model.CurrencyAmount,
                CurrencyCode = model.CurrencyCode,
                TransactionDeviceSource = model.TransactionDeviceSource,
                CustomerName = model.BillingCustomerName,
                Expiry = model.Expiry,
                Cvv = model.CVV,
                MerchantId = model.MerchantId,
                Pan = model.CreditCardNo
            });
            return new ServiceResponse()
            {
                Code = response.ResultCode,
                Message = response.ResultDetail,
                IsSuccess = response.ResultCode == "0000" ? true : false
            };
        }
        private ServiceResponse _HalkBankVPOS(Halkbank.HalkbankVPOSModel model)
        {
            var client = new ePayment.cc5payment()
            {
                host = model.Host,
                name = model.Username,
                password = model.Password,
                bname = model.BillingCustomerName,
                clientid = model.MerchantId,
                subtotal = model.CurrencyAmount.ToString(),
                currency = model.CurrencyCode.ToString(),
                cardnumber = model.CreditCardNo,
                cv2 = model.CVV,
                expmonth = model.ExpiryMonth,
                expyear = model.ExpiryYear,
                chargetype = model.ChargeType
            };
            var pay = client.processorder();
            return new ServiceResponse()
            {
                Code = client.Extra("ERRORCODE"),
                Message = client.errmsg,
                IsSuccess = client.appr == "Approved"
            };
        }
        private ServiceResponse _FinansVPOS(Finansbank.FinansbankVPOSModel model)
        {
            return Finansbank.FinansbankVPOSModel.FinansPostForm(model);
        }
        private ServiceResponse _ZiraatVPOS(Ziraatbank.ZiraatVPOSModel model)
        {
            return new ServiceResponse()
            {
                Code = "999",
                IsSuccess = false,
                Message = "System Error"
            };
        }
        private ServiceResponse _PayTRVPOS(PayTR.PayTRVPOSModel model)
        {
            return new ServiceResponse()
            {
                Code = "999",
                IsSuccess = false,
                Message = "System Error"
            };
        }
    }
}
