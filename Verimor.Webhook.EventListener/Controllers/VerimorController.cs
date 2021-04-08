using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Verimor.Webhook.EventListener.Enums;
using Verimor.Webhook.EventListener.Models;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class VerimorController : Controller
    {
        // GET: Verimor
        RadiusR_NetSpeed_5Entities db = new RadiusR_NetSpeed_5Entities();
        RadiusR.DB.RadiusREntities RadiusREntities = new RadiusR.DB.RadiusREntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Process(WebHookResponse webHookGetEvents)  //response
        {
            try
            {
                while (true)
                {
                    var Parent = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID);
                    int? ParentID = Parent == null ? null : (int?)Convert.ToInt32(Parent);

                    var OperationResp = db.VerimorOperationResponses.Where(m => m.digit == webHookGetEvents.digits && m.ParentID == ParentID).Select(m => m.OperationID).FirstOrDefault();
                    if (webHookGetEvents.digits != null && webHookGetEvents.digits.Length > 1)
                    {
                        OperationResp = db.VerimorOperationResponses.Where(m => m.ParentID == ParentID && m.digit == null).Select(m => m.OperationID).FirstOrDefault();
                    }
                    var Operation = db.VerimorOperations.Find(OperationResp);
                    if (Operation == null)
                    {
                        var prompt = new WebHookRequest.Prompt //request
                        {
                            phrase = string.Format(Common.WrongDialingPhrase, CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing)),
                            max_digits = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMaxDigits),
                            min_digits = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMinDigits),
                            retry_count = Properties.Settings.Default.RetryCount
                        };
                        return Json(WebHookRequestFactory.Prompt(prompt), JsonRequestBehavior.AllowGet);
                    }
                    if (Operation.operationType == (int)VerimorOperationTypes.Basic)
                    {
                        var OpPhrase = StringFormats.GetFormats(Operation.phrase, webHookGetEvents);
                        CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing, OpPhrase);
                        CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMaxDigits, Operation.max_digits);
                        CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMinDigits, Operation.min_digits);
                        var prompt = new WebHookRequest.Prompt //request
                        {
                            phrase = OpPhrase,
                            max_digits = Operation.max_digits,
                            min_digits = Operation.min_digits,
                            retry_count = Properties.Settings.Default.RetryCount
                        };
                        var transfer = new WebHookRequest.Transfer
                        {
                            greet_phrase = Operation.phrase,
                            target = Operation.target
                        };
                        if (transfer.target == null)
                        {
                            CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString());
                            return Json(WebHookRequestFactory.Prompt(prompt), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString()); // null verilebilir görüşme sonu 
                            return Json(WebHookRequestFactory.Transfer(transfer), JsonRequestBehavior.AllowGet);
                        }
                    }
                    // condition
                    WebHookOperations webHookOperations = new WebHookOperations((VerimorOperationTypes)Operation.operationType, webHookGetEvents, RadiusREntities, Operation.Condition, db);
                    CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString());
                    webHookGetEvents.digits = StringFormats.BooleanToInteger(webHookOperations.WebHookOperationResult()).ToString();
                }
            }
            catch (Exception ex)
            {
                var transfer = new WebHookRequest.Transfer
                {
                    greet_phrase = Common.ExceptionPhrase,
                    target = Verimor.Webhook.EventListener.Properties.Settings.Default.ExceptionTarget
                };
                // error phrase and error target in settings
                return Json(WebHookRequestFactory.Transfer(transfer), JsonRequestBehavior.AllowGet);
            }
        }
    }
}