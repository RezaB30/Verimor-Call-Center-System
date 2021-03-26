using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Verimor.Webhook.EventListener.Enums
{
    public enum VerimorOperationTypes
    {
        Basic = 1,
        GetCreditCardNo = 2,
        GetCreditCardDate = 3,
        GetCreditCardCVV = 4,
        GetSubscriptionInfoWithPhoneNumber = 5,
        GetSubscriptionTCKorSubscriptionNo = 6,
        GetUnpaidBills = 7,
        GetUnpaidBillsInPaymentMenu = 8,
        SendCustomerModemInfo = 9,
        GetSubscriptionInfoWithSubscriberNo = 10,
        GetSubscriptionCount = 11,
        TTGeneralFaultQuery = 12,
        CancelledSubscriptionHasUnpaidBills = 13,
        AddCreditCard = 14,
        AutomaticPayment = 15,
        GetPaymentType = 16,
    }    
}
