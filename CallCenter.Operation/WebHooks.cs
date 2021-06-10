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
    public abstract class WebHooks
    {
        private Models.RadiusR_NetSpeed_5Entities VerimorDb = new Models.RadiusR_NetSpeed_5Entities();
        private WebHookResponse WebHookResponse { get; set; }
        private RadiusR.DB.RadiusREntities db { get; set; }
        private ConditionTypes? ConditionTypes { get; set; }
        public long? ConditionParameters { get; set; }
        public WebHooks(WebHookResponse webHookResponse, RadiusR.DB.RadiusREntities db, string condition)
        {
            WebHookResponse = webHookResponse;
            if (string.IsNullOrEmpty(condition))
            {
                ConditionTypes = null;
                ConditionParameters = null;
            }
            else
            {
                ConditionTypes = (ConditionTypes?)short.Parse(condition.Split(',')[0]);
                ConditionParameters = condition.Split(',')[1] == null ? null : long.Parse(condition.Split(',')[1]);
            }
            this.db = db;
        }
        public bool? GetCreditCardNo()
        {
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardNo, WebHookResponse.digits);
            return true;
        }
        public bool? GetCreditCardDate()
        {
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardExpDate, WebHookResponse.digits);
            return true;
        }
        public bool? GetCreditCardCVV()
        {
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardCVV, WebHookResponse.digits);
            return true;
        }
        public bool? GetSubscriptionInfoWithPhoneNumber()
        {
            var PhoneNumber = WebHookResponse.cli.StartsWith("0") ? WebHookResponse.cli.Substring(1, WebHookResponse.cli.Length - 1) : WebHookResponse.cli;
            var subscription = db.Customers
                 .Where(c => c.CustomerIDCard.TCKNo == WebHookResponse.digits || c.ContactPhoneNo == PhoneNumber)
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
                return false;
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
                return true;
            }
        }
        public bool? GetSubscriptionTCKorSubscriptionNo()
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
                return false;
            }
            if (hasCustomer.Length == 1)
            {
                CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, hasCustomer.Select(s => s.SubscriberNo).FirstOrDefault());
            }
            // get subscriber no
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList, string.Join(",", hasCustomer.Select(s => s.SubscriberNo).ToArray()));
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberName, $"{hasCustomer.FirstOrDefault().Customer.FirstName} {hasCustomer.FirstOrDefault().Customer.LastName}");
            return true;
        }
        public bool? GetUnpaidBills()
        {
            var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
            var unpaidBills = db.Bills.Where(b => b.PayDate == null && b.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid && subscriptions.Contains(b.Subscription.SubscriberNo)).ToList();
            if (unpaidBills == null || unpaidBills.Count() == 0)
            {
                return false; // no unpaid bill
            }
            //
            var creditsAmount = db.Subscriptions.Where(s => subscriptions.Contains(s.SubscriberNo)).Sum(s => s.SubscriptionCredits.Select(sc => sc.Amount).DefaultIfEmpty(0).Sum());
            var billsAmount = unpaidBills.Sum(bill => bill.GetPayableCost());
            var payableAmount = Math.Max(0m, billsAmount - creditsAmount);
            //
            //var totalCost = unpaidBills.Select(bill => bill.BillFees.Sum(fee => fee.CurrentCost)).Sum();
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillList, string.Join(",", unpaidBills.Select(bill => bill.ID)));
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.DebtAmount, payableAmount.ToString());
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillCount, unpaidBills.Count().ToString());
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillSubscriptionCount, unpaidBills.GroupBy(bill => bill.SubscriptionID).Count().ToString());
            return true; // have subscriptions unpaid bills 
        }
        public bool? IsPassiveInternet() // need condition
        {
            var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
            var unpaidBills = db.Bills.Where(b => b.PayDate == null && b.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid && subscriptions.Contains(b.Subscription.SubscriberNo)).ToList();
            //if (unpaidBills == null)
            //{
            //    return false;
            //}
            var subscriptionUnpaidBills = unpaidBills?.GroupBy(bill => bill.SubscriptionID).Select(bill => bill.Count());
            foreach (var item in subscriptionUnpaidBills)
            {
                var response = ConditionFormat(item, 1, RadiusR.Verimor.CallCenter.Enums.ConditionTypes.Bigger);
                if (!response)
                {
                    return false;
                }
            }
            return true;
        }
        public bool? HaveUnpaidBillForMoreSubscriptions()
        {
            var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
            var unpaidBills = db.Bills.Where(b => b.PayDate == null && b.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid && subscriptions.Contains(b.Subscription.SubscriberNo)).ToList();
            //var response = ConditionFormat(unpaidBills.GroupBy(bill => bill.SubscriptionID).Count(), 1, Enums.ConditionTypes.Bigger);
            //return response;
            if (unpaidBills == null)
            {
                return false;
            }
            if (unpaidBills.GroupBy(bill => bill.SubscriptionID).Count() > 1)
            {
                return true; // has more subscriptions with unpaid bills
            }
            return false;
        }
        public bool? SendCustomerModemInfo()
        {
            var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            if (subscriberNo == null)
            {
                return false;
            }
            SMSService service = new SMSService();
            var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
            if (subscription == null)
            {
                return false;
            }
            var modemInfoSMS = db.SMSTexts.Where(sms => sms.TypeID == (int)RadiusR.DB.Enums.SMSType.UserCredentials && !sms.IsDisabled && sms.Culture == subscription.Customer.Culture).FirstOrDefault();
            if (modemInfoSMS == null)
            {
                var modemCredentialsText = Common.ResourceManager.GetString("UserCredentialsText", CultureInfo.CreateSpecificCulture(subscription.Customer.Culture))
                    .Replace("([username])", subscription.RadiusAuthorization.Username)
                    .Replace("([password])", subscription.RadiusAuthorization.Password)
                    .Replace("([subscriberNo])", subscriberNo);
                service.SendGenericSMS(subscription.Customer.ContactPhoneNo, subscription.Customer.Culture, rawText: modemCredentialsText);
            }
            else
            {
                var modemCredentialsText = modemInfoSMS.Text
                    .Replace("([username])", subscription.RadiusAuthorization.Username)
                    .Replace("([password])", subscription.RadiusAuthorization.Password)
                    .Replace("([subscriberNo])", subscriberNo);
                service.SendGenericSMS(subscription.Customer.ContactPhoneNo, subscription.Customer.Culture, rawText: modemCredentialsText);
            }
            return true;
        }
        public bool? GetSubscriptionInfoWithSubscriberNo()
        {
            var subscriberNo = WebHookResponse.digits;
            var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
            if (subscription == null)
            {
                return false;
            }
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscription.SubscriberNo);
            return true;
        }
        public bool? GetSubscriptionCount() // abone sayısı kontrolü
        {
            var subscriptionCount = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
            if (subscriptionCount.Length == 1)
            {
                CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscriptionCount.FirstOrDefault());
            }
            var response = ConditionFormat(subscriptionCount.Length, 1, RadiusR.Verimor.CallCenter.Enums.ConditionTypes.Bigger);
            return response;
        }
        public bool? TTGeneralFaultQuery()
        {
            // need domain cache module
            var TTCredentials = db.TelekomAccessCredentials.Find(1);
            OLOServiceClient client = new OLOServiceClient(TTCredentials.OLOPortalUsername, TTCredentials.OLOPortalPassword);
            var subscriptionNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            var response = client.ListGeneralFaultsBySubscriptionNo(subscriptionNo);
            if (response.InternalException != null)
            {
                return false;
            }
            var faultEndDate = response.Data.Select(f => f.EndDate).OrderByDescending(f => f).FirstOrDefault();
            if (faultEndDate == null)
            {
                return false;
            }
            // convert date and read as date 
            var message = $"Sayın müşterimiz bölgenizde genel arıza çalışması bulunmaktadır. Çalışmanın bitiş tarihi {faultEndDate}";
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.GeneralFault, message);
            return true;
        }
        public bool? CancelledSubscriptionHasUnpaidBills()
        {
            var subscriptions = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.CancelledSubscriptions).Split(',');
            if (subscriptions == null || subscriptions.Length == 0)
            {
                return false;
            }
            if (subscriptions.Length == 1)
            {
                CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo, subscriptions.FirstOrDefault());
            }
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList, string.Join(",", subscriptions));
            return true;
        }
        public bool? AddCreditCard()
        {
            if (!MobilExpressSettings.MobilExpressIsActive)
            {
                return false;
            }
            var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
            if (subscription == null)
            {
                return false;
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
                return false; // error
            }
            if (response.Response.ResponseCode == RezaB.API.MobilExpress.Response.ResponseCodes.DuplicateCard)
            {
                return false; // already registered card 
            }
            if (response.Response.ResponseCode == RezaB.API.MobilExpress.Response.ResponseCodes.Success)
            {
                CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardToken, response.Response.CardToken);
                db.SystemLogs.Add(SystemLogProcessor.AddCreditCard(subscription.CustomerID, SystemLogInterface.CustomerWebsite, subscriberNo, cardNo.Substring(0, 6) + "******" + cardNo.Substring(12)));
                db.SaveChanges();
                return true; // added card
            }
            //
            return false; // error
        }
        public bool? AutomaticPayment()
        {
            if (!MobilExpressSettings.MobilExpressIsActive)
            {
                return false;
            }
            var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
            if (subscription == null)
            {
                return false;
            }
            var client = new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.MobilExpressAdapterClient(MobilExpressSettings.MobilExpressMerchantKey, MobilExpressSettings.MobilExpressAPIPassword, new RadiusR.API.MobilExpress.DBAdapter.AdapterClient.ClientConnectionDetails()
            {
                IP = HttpContext.Current.Request.UserHostAddress,
                UserAgent = HttpContext.Current.Request.UserAgent
            });
            var response = client.GetCards(subscription.Customer);
            if (response.InternalException != null)
            {
                return false;
            }
            if (response.Response.ResponseCode != RezaB.API.MobilExpress.Response.ResponseCodes.Success)
            {
                return false;
            }
            var cardToken = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.PaymentCardToken);
            short paymentType = (short)RadiusR.DB.Enums.AutoPaymentType.OnBillExpiration;
            var currentCard = response.Response.CardList.FirstOrDefault(c => c.CardToken == cardToken);
            if (currentCard == null)
            {
                return false;
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
            return true;
        }
        public bool? PayBills()
        {
            var getUnpaidBills = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.UnpaidBillList);
            if (string.IsNullOrEmpty(getUnpaidBills))
            {
                return false;
            }
            var unpaidBills = getUnpaidBills.Split(',');
            var payableAmount = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.DebtAmount);
            if (string.IsNullOrEmpty(payableAmount))
            {
                return false;
            }
            var payBills = BillPayment(WebHookResponse.uuid);
            if (!payBills)
            {
                //log
                return false;
            }
            var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            var bills = db.Bills.Where(bill => unpaidBills.Contains(bill.ID.ToString())).ToArray();
            var paymentResponse = RadiusR.DB.Utilities.Billing.BillPayment.PayBills(db, bills, PaymentType.VirtualPos, RadiusR.DB.Utilities.Billing.BillPayment.AccountantType.Admin, null, null);
            db.SystemLogs.Add(RadiusR.SystemLogs.SystemLogProcessor.BillPayment(bills.Select(b => b.ID).ToArray(), null, bills.FirstOrDefault().SubscriptionID, SystemLogInterface.CustomerWebsite, subscriberNo, PaymentType.VirtualPos));
            db.SaveChanges();
            if (paymentResponse == RadiusR.DB.Utilities.Billing.BillPayment.ResponseType.Success)
            {
                return true;
            }
            return false;
        }
        public bool? SendValidationSMS()
        {
            var subscriberNo = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriberNo);
            var subscription = db.Subscriptions.Where(s => s.SubscriberNo == subscriberNo).FirstOrDefault();
            if (subscription == null)
            {
                return false;
            }
            var rand = new Random();
            var smsCode = rand.Next(100000, 999999).ToString();
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.ValidationSMS, smsCode);
            SMSService smsClient = new SMSService();
            var smsResponse = smsClient.SendSubscriberSMS(subscription, SMSType.OperationCode, new Dictionary<string, object>() {
                        { SMSParamaterRepository.SMSParameterNameCollection.SMSCode, smsCode }
                    });
            return true;
        }
        public bool? ValidationSMS()
        {
            var customerSMSCode = WebHookResponse.digits;
            var smsCode = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.ValidationSMS);
            if (smsCode == customerSMSCode)
            {
                return true;
            }
            return false;
        }
        public bool? CompanyGeneralFault()
        {
            var subscriptionList = CacheManager.Get(WebHookResponse.uuid, CacheManager.CacheItemType.SubscriptionList).Split(',');
            var subscription = db.Subscriptions.Where(s => subscriptionList.Contains(s.SubscriberNo)).ToArray();
            if (subscription == null || subscription.Length == 0)
            {
                return false;
            }
            var subscriberProvince = subscription.Select(s => s.Address.ProvinceID).ToArray();
            var generalFaults = VerimorDb.GeneralFaults.Where(gf => gf.EndTime > DateTime.Now && subscriberProvince.Contains(gf.Province)).ToArray();
            if (generalFaults == null || generalFaults.Length == 0)
            {
                return false;
            }

            var startDateTime = generalFaults.FirstOrDefault().StartTime.ToString("dd MMMM yyyy HH mm");
            var endDateTime = generalFaults.FirstOrDefault().EndTime.ToString("dd MMMM yyyy HH mm");
            var description = generalFaults.FirstOrDefault().Description;

            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.CompanyGeneralFault, description);
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.CompanyGeneralFaultStartTime, startDateTime);
            CacheManager.Add(WebHookResponse.uuid, CacheManager.CacheItemType.CompanyGeneralFaultEndTime, endDateTime);
            return true;
        }
        #region private functions
        private bool BillPayment(string uuid)
        {
            //var client = new RadiusR.VPOS.Custom.VPOSPaymentClient(VPOSSettings.CurrentVPOSID, VPOSSettings.MerchantID, VPOSSettings.StoreKey, VPOSSettings.MerchantSalt, VPOSSettings.UserID, VPOSSettings.UserPassword);
            var client = new RadiusR.VPOS.Custom.VPOSPaymentClient(CustomVPOSSettings.CurrentVPOSID, CustomVPOSSettings.MerchantID, CustomVPOSSettings.StoreKey, CustomVPOSSettings.MerchantSalt, CustomVPOSSettings.UserID, CustomVPOSSettings.UserPassword);
            var response = client.Payment(new RadiusR.VPOS.Custom.VPOSPaymentRequest()
            {
                ClientIP = HttpContext.Current.Request.UserHostAddress,
                CreditCardNo = CacheManager.Get(uuid, CacheManager.CacheItemType.PaymentCardNo),
                CustomerName = CacheManager.Get(uuid, CacheManager.CacheItemType.SubscriberName),
                Description = string.Empty,
                CurrencyAmount = Convert.ToDecimal(CacheManager.Get(uuid, CacheManager.CacheItemType.DebtAmount), CultureInfo.InvariantCulture),
                CVV = CacheManager.Get(uuid, CacheManager.CacheItemType.PaymentCardCVV),
                ExpiryYear = CacheManager.Get(uuid, CacheManager.CacheItemType.PaymentCardExpDate).Substring(2, 2),
                ExpiryMonth = CacheManager.Get(uuid, CacheManager.CacheItemType.PaymentCardExpDate).Substring(0, 2),
            });
            return response.IsSuccess;
        }
        private bool ConditionFormat(int value, long conditionParameters, ConditionTypes conditionTypes)
        {
            switch (ConditionTypes)
            {
                case RadiusR.Verimor.CallCenter.Enums.ConditionTypes.Bigger:
                    {
                        if (value > ConditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RadiusR.Verimor.CallCenter.Enums.ConditionTypes.Equal:
                    {
                        if (value == ConditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RadiusR.Verimor.CallCenter.Enums.ConditionTypes.Smaller:
                    {
                        if (value < ConditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RadiusR.Verimor.CallCenter.Enums.ConditionTypes.GreaterEqual:
                    {
                        if (value >= ConditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case RadiusR.Verimor.CallCenter.Enums.ConditionTypes.LittleEqual:
                    {
                        if (value <= ConditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case null:
                    {
                        if (value <= conditionParameters)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    if (value <= conditionParameters)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
        #endregion
    }
}
