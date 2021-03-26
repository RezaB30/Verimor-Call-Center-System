using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult Index()
        {
            var db = new RadiusR.DB.RadiusREntities();

            RadiusR.VPOS.Custom.VPOSPaymentClient payment = new RadiusR.VPOS.Custom.VPOSPaymentClient(RadiusR.DB.VPOSSettings.CurrentVPOSID, RadiusR.DB.VPOSSettings.MerchantID, RadiusR.DB.VPOSSettings.StoreKey, RadiusR.DB.VPOSSettings.MerchantSalt);
            var response = payment.Payment(00.20m, "", "", "185.188.129.2", "5487930090053959", "108", "08","2029");
            return View();
        }
    }
}