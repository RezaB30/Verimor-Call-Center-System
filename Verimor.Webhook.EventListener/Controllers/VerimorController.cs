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
                            phrase = "Yanlış tuşlama yaptınız. " + CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing),
                            max_digits = "12",
                            min_digits = "1",
                            retry_count = "2"
                        };
                        return Json(WebHookRequestFactory.Prompt(prompt), JsonRequestBehavior.AllowGet);
                    }
                    if (Operation.operationType == (int)VerimorOperationTypes.Basic)
                    {
                        var OpPhrase = StringFormats.GetFormats(Operation.phrase, webHookGetEvents);
                        CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing, OpPhrase);
                        var prompt = new WebHookRequest.Prompt //request
                        {
                            phrase = OpPhrase,
                            announcement_id = Operation.announcementID == null ? null : Operation.announcementID.ToString(),
                            max_digits = Operation.max_digits,
                            min_digits = Operation.min_digits,
                            retry_count = Operation.retry_count
                        };
                        var transfer = new WebHookRequest.Transfer
                        {
                            greet_phrase = Operation.phrase,
                            target = Operation.target
                        };
                        if (prompt.retry_count != null)
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
                    WebHookOperations webHookOperations = new WebHookOperations((VerimorOperationTypes)Operation.operationType, webHookGetEvents, RadiusREntities);
                    CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString());
                    webHookGetEvents.digits = webHookOperations.WebHookOperationResult().ToString();


                    //if (Operation.operationType == (int)VerimorOperationQueryTypes.GetSubscriptionInfoWithPhoneNumber)
                    //{
                    //    CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString());
                    //    var Result = webHookOperations.GetSubscriptionInfoWithPhoneNumber(webHookGetEvents, db);
                    //    webHookGetEvents.digits = Result.ToString();
                    //}
                }
            }
            catch (Exception ex)
            {
                //CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, null);
                var transfer = new WebHookRequest.Transfer
                {
                    greet_phrase = "İşlem sırasında bir hata oluştu. Sizi müşteri temsilcisine aktarıyorum. Lütfen Bekleyiniz.",
                    target = "queue/201"
                };
                return Json(WebHookRequestFactory.Transfer(transfer), JsonRequestBehavior.AllowGet);
            }
        }
    }
}