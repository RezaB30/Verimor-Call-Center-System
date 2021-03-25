using RadiusR.DB;
using RadiusR.DB.Enums;
using RadiusR.DB.ModelExtentions;
using RadiusR.DB.Utilities.Billing;
using RadiusR.SMS;
using RadiusR.SystemLogs;
using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using RezaB.TurkTelekom.WebServices.OLO;
using RezaB.TurkTelekom.WebServices.TTApplication;
using RezaB.TurkTelekom.WebServices.TTOYS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Verimor.Webhook.EventListener.Enums;
using Verimor.Webhook.EventListener.Models;

namespace Verimor.Webhook.EventListener
{
    public class WebHookOperations
    {
        private VerimorOperationQueryTypes OperationQueryType { get; set; }
        private WebHookResponse WebHookResponse { get; set; }
        private RadiusR.DB.RadiusREntities db { get; set; }
        public WebHookOperations(VerimorOperationQueryTypes operationType, WebHookResponse webHookResponse, RadiusR.DB.RadiusREntities db)
        {
            OperationQueryType = operationType;
            WebHookResponse = webHookResponse;
            this.db = db;
        }
        public int? WebHookOperationResult()
        {
            switch (OperationQueryType)
            {
                case VerimorOperationQueryTypes.Basic:
                    {
                        return null;
                    }
                case VerimorOperationQueryTypes.GetCreditCardNo:
                    {
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardNo, WebHookResponse.digits);
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetCreditCardDate:
                    {
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardExpDate, WebHookResponse.digits);
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetCreditCardCVV:
                    {
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardCVV, WebHookResponse.digits);
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetSubscriptionInfoWithPhoneNumber: // first calling
                    {
                        var PhoneNumber = WebHookResponse.cli.StartsWith("0") ? WebHookResponse.cli.Substring(1, WebHookResponse.cli.Length - 1) : WebHookResponse.cli;
                        var subscription = db.Customers
                             .Where(c => c.CustomerIDCard.TCKNo == WebHookResponse.digits || c.Subscriptions.Where(s => s.SubscriberNo == WebHookResponse.digits).Any())
                             .SelectMany(c => c.Subscriptions).ToArray();

                        subscription = subscription.Where(s => s.State == (short)CustomerState.Active
                            || s.State == (int)CustomerState.Registered
                            || s.State == (int)CustomerState.Reserved
                            || s.State == (int)CustomerState.Disabled).ToArray();
                        var hasCancelledSubscription = db.Customers.Where(c => c.ContactPhoneNo == PhoneNumber
                        && c.Subscriptions.Where(s => s.State == (int)CustomerState.Cancelled
                        && s.Bills.Where(b => b.BillStatusID == (short)BillState.Unpaid
                        && b.PayDate == null).Any()).Any()).SelectMany(c => c.Subscriptions).ToList();
                        if (hasCancelledSubscription != null && hasCancelledSubscription.Count() > 0)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.CancelledSubscriptions, string.Join(",", hasCancelledSubscription.Select(s => s.SubscriberNo).ToArray()));
                        }
                        if (subscription.Length == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            //get subscriber no
                            if (subscription.Length == 1)
                            {
                                CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscription.Select(s => s.SubscriberNo).FirstOrDefault());
                            }
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList, string.Join(",", subscription.Select(s => s.SubscriberNo).ToArray()));
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberName, $"{subscription.FirstOrDefault().Customer.FirstName} {subscription.FirstOrDefault().Customer.LastName}");
                            return 1;
                        }
                    }
                case VerimorOperationQueryTypes.GetSubscriptionTCKorSubscriptionNo: // customer want subscription operation
                    {
                        var hasCustomer = db.Customers
                            .Where(c => c.CustomerIDCard.TCKNo == WebHookResponse.digits || c.Subscriptions.Where(s => s.SubscriberNo == WebHookResponse.digits).Any())
                            .SelectMany(c => c.Subscriptions).ToArray();

                        hasCustomer = hasCustomer.Where(s => s.State == (short)CustomerState.Active
                            || s.State == (int)CustomerState.Registered
                            || s.State == (int)CustomerState.Reserved
                            || s.State == (int)CustomerState.Disabled).ToArray();

                        var hasCancelledSubscription = db.Customers.Where(c => c.ContactPhoneNo == WebHookResponse.digits
                        && c.Subscriptions.Where(s => s.State == (int)CustomerState.Cancelled
                        && s.Bills.Where(b => b.BillStatusID == (short)BillState.Unpaid
                        && b.PayDate == null).Any()).Any()).SelectMany(c => c.Subscriptions).ToList();
                        if (hasCancelledSubscription != null && hasCancelledSubscription.Count() > 0)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.CancelledSubscriptions, string.Join(",", hasCancelledSubscription.Select(s => s.SubscriberNo).ToArray()));
                        }
                        if (hasCustomer.Length == 0)
                        {
                            return 0;
                        }
                        if (hasCustomer.Length == 1)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, hasCustomer.Select(s => s.SubscriberNo).FirstOrDefault());
                        }
                        // get subscriber no
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList, string.Join(",", hasCustomer.Select(s => s.SubscriberNo).ToArray()));
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberName, $"{hasCustomer.FirstOrDefault().Customer.FirstName} {hasCustomer.FirstOrDefault().Customer.LastName}");
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetUnpaidBillsInPaymentMenu:
                case VerimorOperationQueryTypes.GetUnpaidBills:
                    {
                        var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
                        var unpaidBills = db.Bills.Where(b => b.PayDate == null && b.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid && subscriptions.Contains(b.Subscription.SubscriberNo)).ToList();
                        if (unpaidBills == null || unpaidBills.Count() == 0)
                        {
                            return 0; // no unpaid bill
                        }
                        //
                        var creditsAmount = db.Subscriptions.Where(s => subscriptions.Contains(s.SubscriberNo)).Sum(s => s.SubscriptionCredits.Select(sc => sc.Amount).DefaultIfEmpty(0).Sum());
                        var billsAmount = unpaidBills.Sum(bill => bill.GetPayableCost());
                        var payableAmount = Math.Max(0m, billsAmount - creditsAmount);
                        //
                        //var totalCost = unpaidBills.Select(bill => bill.BillFees.Sum(fee => fee.CurrentCost)).Sum();
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.DebtAmount, payableAmount.ToString());
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillCount, unpaidBills.Count().ToString());
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillSubscriptionCount, unpaidBills.GroupBy(bill => bill.SubscriptionID).Count().ToString());
                        var subscriptionUnpaidBills = unpaidBills.GroupBy(bill => bill.SubscriptionID).Select(bill => bill.Count());
                        if (!subscriptionUnpaidBills.Contains(1))
                        {
                            return 3; // All subscriptions is passive (one or more)
                        }
                        if (unpaidBills.GroupBy(bill => bill.SubscriptionID).Count() > 1)
                        {
                            return 2; // has more subscriptions with unpaid bills
                        }
                        return 1; // have subscriptions one unpaid bills 
                    }
                case VerimorOperationQueryTypes.SendCustomerModemInfo:
                    {
                        var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
                        if (subscriberNo == null)
                        {
                            return 0;
                        }
                        SMSService service = new SMSService();
                        var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
                        if (subscription == null)
                        {
                            return 0;
                        }
                        service.SendGenericSMS(subscription.Customer.ContactPhoneNo, subscription.Customer.Culture, rawText: string.Format("Sayın Müşterimiz modem kullanıcı adınız : {0} , şifreniz : {1} . İyi günler dileriz.", subscription.Username, subscription.RadiusPassword));
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetSubscriptionInfoWithSubscriberNo:
                    {
                        var subscriberNo = WebHookResponse.digits;
                        var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
                        if (subscription == null)
                        {
                            return 0;
                        }
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscription.SubscriberNo);
                        return 1;
                    }
                case VerimorOperationQueryTypes.GetSubscriptionCount:
                    {
                        var subscriptionCount = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
                        if (subscriptionCount.Length > 1)
                        {
                            return 0;
                        }
                        if (subscriptionCount.Length == 1)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscriptionCount.FirstOrDefault());
                            return 1;
                        }
                        return 0; // get subscriber no from 
                    }
                case VerimorOperationQueryTypes.TTGeneralFaultQuery:
                    {
                        // need domain cache module
                        var TTCredentials = db.TelekomAccessCredentials.Find(1);
                        OLOServiceClient client = new OLOServiceClient(TTCredentials.OLOPortalUsername, TTCredentials.OLOPortalPassword);
                        var subscriptionNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
                        var response = client.ListGeneralFaultsBySubscriptionNo(subscriptionNo);
                        if (response.InternalException != null)
                        {
                            return 0;
                        }
                        var faultEndDate = response.Data.Select(f => f.EndDate).OrderByDescending(f => f).FirstOrDefault();
                        if (faultEndDate == null)
                        {
                            return 0;
                        }
                        // convert date and read as date 
                        var message = $"Sayın müşterimiz bölgenizde genel arıza çalışması bulunmaktadır. Çalışmanın bitiş tarihi {faultEndDate}";
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.GeneralFault, message);
                        return 1;
                    }
                case VerimorOperationQueryTypes.CancelledSubscriptionHasUnpaidBills:
                    {
                        var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.CancelledSubscriptions).Split(',');
                        if (subscriptions == null || subscriptions.Length == 0)
                        {
                            return 0;
                        }
                        if (subscriptions.Length == 1)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscriptions.FirstOrDefault());
                        }
                        CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList, string.Join(",", subscriptions));
                        return 1;
                    }
                case VerimorOperationQueryTypes.AddCreditCard:
                    {
                        if (!MobilExpressSettings.MobilExpressIsActive)
                        {
                            return 0;
                        }
                        var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
                        var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
                        if (subscription == null)
                        {
                            return 0;
                        }
                        var cardHolderName = $"{subscription.Customer.FirstName} {subscription.Customer.LastName}";
                        var cardNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardNo);
                        var cardExpiredDate = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardExpDate);
                        var cardExpiredMonth = int.Parse(cardExpiredDate.Substring(0, 2));
                        var cardExpiredYear = int.Parse("20" + cardExpiredDate.Substring(2, 2));
                        var client = new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.MobilExpressAdapterClient(MobilExpressSettings.MobilExpressMerchantKey, MobilExpressSettings.MobilExpressAPIPassword, new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.ClientConnectionDetails()
                        {
                            IP = HttpContext.Current.Request.UserHostAddress,
                            UserAgent = HttpContext.Current.Request.UserAgent
                        });
                        var response = client.SaveCard(subscription.Customer, new RadiusR.API.MobilExpress.DBAdapter.AdapterParameters.AdapterCard()
                        {
                            CardHolderName = cardHolderName,
                            CardMonth = cardExpiredMonth,
                            CardYear = cardExpiredYear,
                            CardNumber = cardNo
                        });
                        if (response.InternalException != null)
                        {
                            return 0; // error
                        }
                        if (response.Response.ResponseCode == RezaB.API.MobilExpress.Response.ResponseCodes.DuplicateCard)
                        {
                            return 1; // already registered card 
                        }
                        if (response.Response.ResponseCode == RezaB.API.MobilExpress.Response.ResponseCodes.Success)
                        {
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardToken, response.Response.CardToken);
                            db.SystemLogs.Add(SystemLogProcessor.AddCreditCard(subscription.CustomerID, SystemLogInterface.CustomerWebsite, subscriberNo, cardNo.Substring(0, 6) + "******" + cardNo.Substring(12)));
                            db.SaveChanges();
                            return 2; // added card
                        }
                        //
                        return 0; // error
                    }
                case VerimorOperationQueryTypes.GetPaymentType:
                    {
                        var digit = Convert.ToInt32(WebHookResponse.digits);
                        if (digit == 1)
                        {
                            var type = (short)AutoPaymentType.OnBillIssue;
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentType, $"{type}");
                        }
                        if (digit == 2)
                        {
                            var type = (short)AutoPaymentType.OnBillExpiration;
                            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentType, $"{type}");
                        }
                        return 1;
                    }
                case VerimorOperationQueryTypes.AutomaticPayment:
                    {
                        if (!MobilExpressSettings.MobilExpressIsActive)
                        {
                            return 0;
                        }
                        var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
                        var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
                        if (subscription == null)
                        {
                            return 0;
                        }
                        var client = new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.MobilExpressAdapterClient(MobilExpressSettings.MobilExpressMerchantKey, MobilExpressSettings.MobilExpressAPIPassword, new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.ClientConnectionDetails()
                        {
                            IP = HttpContext.Current.Request.UserHostAddress,
                            UserAgent = HttpContext.Current.Request.UserAgent
                        });
                        var response = client.GetCards(subscription.Customer);
                        if (response.InternalException != null)
                        {
                            return 0;
                        }
                        if (response.Response.ResponseCode != RezaB.API.MobilExpress.Response.ResponseCodes.Success)
                        {
                            return 0;
                        }
                        var cardToken = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardToken);
                        short paymentType = short.Parse(CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentType));
                        var currentCard = response.Response.CardList.FirstOrDefault(c => c.CardToken == cardToken);
                        if (currentCard == null)
                        {
                            return 0;
                        }
                        subscription.MobilExpressAutoPayment = new RadiusR.DB.MobilExpressAutoPayment()
                        {
                            CardToken = currentCard.CardToken,
                            PaymentType = paymentType
                        };
                        db.SaveChanges();
                        var smsClient = new SMSService();
                        db.SMSArchives.AddSafely(smsClient.SendSubscriberSMS(subscription, SMSType.MobilExpressActivation, new Dictionary<string, object> {
                        { SMSParamaterRepository.SMSParameterNameCollection.CardNo, currentCard.MaskedCardNumber }
                    }));
                        db.SystemLogs.Add(SystemLogProcessor.ActivateAutomaticPayment(subscription.ID, SystemLogInterface.CustomerWebsite, subscriberNo, "MobilExpress"));
                        db.SaveChanges();
                        return 1;
                    }
                default:
                    return null;
            }
        }
    }
}