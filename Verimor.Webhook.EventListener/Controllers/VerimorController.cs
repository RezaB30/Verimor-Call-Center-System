using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class VerimorController : BaseController
    {
        // GET: Verimor
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Process(WebHookResponse webHookGetEvents)  //response
        {
            CallCenter.Operation.EventListenerClient listener = new CallCenter.Operation.EventListenerClient();            
            var response = listener.GetWebHook(webHookGetEvents);
            return Json(response, JsonRequestBehavior.AllowGet);            
        }
    }
}