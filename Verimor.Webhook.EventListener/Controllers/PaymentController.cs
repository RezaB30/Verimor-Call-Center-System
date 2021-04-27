using RadiusR.DB;
using RadiusR.VPOS.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class PaymentController : BaseController
    {
        // GET: Payment
        public ActionResult Index()
        {
            var db = new RadiusR.DB.RadiusREntities();

            RadiusR.VPOS.Custom.VPOSPaymentClient payment = new VPOSPaymentClient(VPOSSettings.CurrentVPOSID, VPOSSettings.MerchantID, VPOSSettings.StoreKey, VPOSSettings.MerchantSalt, VPOSSettings.UserID, VPOSSettings.UserPassword);
            var response = payment.Payment(new VPOSPaymentRequest()
            {
                ExpiryYear = "29",
                ExpiryMonth = "08",
                CVV = "108",
                CurrencyAmount = 0.1m,
                ClientIP = "185.188.129.2",
                CreditCardNo = "5487930090053959",
                CustomerName = "",
                Description = ""
            });
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}