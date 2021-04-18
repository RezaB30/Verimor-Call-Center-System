using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class VerimorController : Controller
    {
        // GET: Verimor
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Process(WebHookResponse webHookGetEvents)  //response
        {
            CallCenter.Operation.EventListener listener = new CallCenter.Operation.EventListener();
            var response = listener.GetWebHook(webHookGetEvents);
            return Json(response, JsonRequestBehavior.AllowGet);            
        }
    }
}