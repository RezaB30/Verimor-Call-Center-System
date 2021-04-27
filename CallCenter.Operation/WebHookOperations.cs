using RadiusR.DB;
using RadiusR.DB.Enums;
using RadiusR.DB.ModelExtentions;
using RadiusR.DB.Utilities.Billing;
using RadiusR.SMS;
using RadiusR.SystemLogs;
using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using RadiusR.Verimor.CallCenter.Enums;
using RezaB.TurkTelekom.WebServices.OLO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CallCenter.Operation
{
    public class WebHookOperations : WebHooks
    {
        private VerimorOperationTypes OperationQueryType { get; set; }
        public WebHookOperations(VerimorOperationTypes operationType, WebHookResponse webHookResponse, RadiusR.DB.RadiusREntities db, string condition)
            : base(webHookResponse, db, condition)
        {
            OperationQueryType = operationType;
        }
        public bool? WebHookOperationResult()
        {
            switch (OperationQueryType)
            {
                case VerimorOperationTypes.Basic:
                    {
                        return null;
                    }
                case VerimorOperationTypes.GetCreditCardNo:
                    {
                        return GetCreditCardNo();
                    }
                case VerimorOperationTypes.GetCreditCardDate:
                    {
                        return GetCreditCardDate();
                    }
                case VerimorOperationTypes.GetCreditCardCVV:
                    {
                        return GetCreditCardCVV();
                    }
                case VerimorOperationTypes.GetSubscriptionInfoWithPhoneNumber: // first calling
                    {
                        return GetSubscriptionInfoWithPhoneNumber();
                    }
                case VerimorOperationTypes.GetSubscriptionTCKorSubscriptionNo: // customer want subscription operation
                    {
                        return GetSubscriptionTCKorSubscriptionNo();
                    }
                case VerimorOperationTypes.GetUnpaidBillsInPaymentMenu:
                case VerimorOperationTypes.GetUnpaidBills:
                    {
                        return GetUnpaidBills();
                    }
                case VerimorOperationTypes.IsPassiveInternet:
                    {
                        return IsPassiveInternet();
                    }
                case VerimorOperationTypes.HaveUnpaidBillForMoreSubscriptions:
                    {
                        return HaveUnpaidBillForMoreSubscriptions();
                    }
                case VerimorOperationTypes.SendCustomerModemInfo:
                    {
                        return SendCustomerModemInfo();
                    }
                case VerimorOperationTypes.GetSubscriptionInfoWithSubscriberNo:
                    {
                        return GetSubscriptionInfoWithSubscriberNo();
                    }
                case VerimorOperationTypes.GetSubscriptionCount:
                    {
                        return GetSubscriptionCount();
                    }
                case VerimorOperationTypes.TTGeneralFaultQuery:
                    {
                        return TTGeneralFaultQuery();
                    }
                case VerimorOperationTypes.CancelledSubscriptionHasUnpaidBills:
                    {
                        return CancelledSubscriptionHasUnpaidBills();
                    }
                case VerimorOperationTypes.AddCreditCard:
                    {
                        return AddCreditCard();
                    }
                case VerimorOperationTypes.AutomaticPayment:
                    {
                        return AutomaticPayment();
                    }
                case VerimorOperationTypes.PayBills:
                    {
                        return PayBills();
                    }
                case VerimorOperationTypes.SendValidationSMS:
                    {
                        return SendValidationSMS();
                    }
                case VerimorOperationTypes.ValidationSMS:
                    {
                        return ValidationSMS();
                    }
                case VerimorOperationTypes.CompanyGeneralFault:
                    {
                        return CompanyGeneralFault();
                    }
                default:
                    return null;
            }
        }
    }
}
