using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener
{
    public interface WebHookBaseOperations<T>
    {
        T CreditCardNo();
        T GetCreditCardDate();
        T GetCreditCardCVV();
        T GetSubscriptionInfoWithPhoneNumber();
        T GetSubscriptionTCKorSubscriptionNo();
        T GetUnpaidBills();
        T GetUnpaidBillsInPaymentMenu();
        T SendCustomerModemInfo();
        T GetSubscriptionInfoWithSubscriberNo();
        T GetSubscriptionCount();
        T TTGeneralFaultQuery();
        T CancelledSubscriptionHasUnpaidBills();
        T AddCreditCard();
        T AutomaticPayment();
        T IsPassiveInternet();
        T HaveUnpaidBillForMoreSubscriptions();
    }
}