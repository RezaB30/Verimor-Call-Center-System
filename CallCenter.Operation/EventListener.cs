using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using RadiusR.Verimor.CallCenter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenter.Operation
{
    public class EventListener
    {
        Models.RadiusR_NetSpeed_5Entities db = new Models.RadiusR_NetSpeed_5Entities();
        RadiusR.DB.RadiusREntities radiusRDb = new RadiusR.DB.RadiusREntities();
        public WebHookRequest GetWebHook(WebHookResponse webHookGetEvents)
        {
            try
            {
                while (true)
                {
                    var Parent = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID);
                    int? ParentID = Parent == null ? null : int.TryParse(Parent,out int currparent) ? currparent : null;

                    var OperationResp = db.VerimorOperationResponses.Where(m => m.digit == webHookGetEvents.digits && m.ParentID == ParentID).Select(m => m.OperationID).FirstOrDefault();
                    if (webHookGetEvents.digits != null && webHookGetEvents.digits.Length > 1)
                    {
                        OperationResp = db.VerimorOperationResponses.Where(m => m.ParentID == ParentID && m.digit == null).Select(m => m.OperationID).FirstOrDefault();
                    }
                    var Operation = db.VerimorOperations.Find(OperationResp);
                    if (Operation == null)
                    {
                        // wrong operation
                        return BasicOperationResponse.GetWrongOperationResponse(webHookGetEvents);                        
                    }
                    if (Operation.operationType == (int)VerimorOperationTypes.Basic)
                    {
                        return BasicOperationResponse.GetBasicOperationResponse(webHookGetEvents, OperationResp, Operation);                        
                    }
                    // condition
                    WebHookOperations webHookOperations = new WebHookOperations((VerimorOperationTypes)Operation.operationType, webHookGetEvents, radiusRDb, Operation.Condition);
                    CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, OperationResp.ToString());
                    webHookGetEvents.digits = StringFormats.BooleanToInteger(webHookOperations.WebHookOperationResult()).ToString();
                }
            }
            catch (Exception ex)
            {
                var transfer = new WebHookRequest.Transfer
                {
                    greet_phrase = Common.ExceptionPhrase,
                    target = Properties.Settings.Default.ExceptionTarget
                };
                // error phrase and error target in settings
                return WebHookRequestFactory.Transfer(transfer);
            }
        }
    }
}
