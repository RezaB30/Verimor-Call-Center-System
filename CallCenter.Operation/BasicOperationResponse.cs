using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenter.Operation
{
    public static class BasicOperationResponse
    {
        public static WebHookRequest GetWrongOperationResponse(WebHookResponse webHookGetEvents)
        {
            var prompt = new WebHookRequest.Prompt //request
            {
                phrase = string.Format(Common.WrongDialingPhrase, CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing)),
                max_digits = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMaxDigits),
                min_digits = CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMinDigits),
                retry_count = Properties.Settings.Default.RetryCount
            };
            return WebHookRequestFactory.Prompt(prompt);
        }
        public static WebHookRequest GetBasicOperationResponse(WebHookResponse webHookGetEvents, int currentOperationId, Models.VerimorOperation operation)
        {
            var OpPhrase = StringFormats.GetFormats(operation.phrase, webHookGetEvents);
            CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialing, OpPhrase);
            CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMaxDigits, operation.max_digits);
            CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.WrongDialingMinDigits, operation.min_digits);
            var prompt = new WebHookRequest.Prompt //request
            {
                phrase = OpPhrase,
                max_digits = operation.max_digits,
                min_digits = operation.min_digits,
                retry_count = Properties.Settings.Default.RetryCount
            };
            var transfer = new WebHookRequest.Transfer
            {
                greet_phrase = operation.phrase,
                target = operation.target
            };
            if (transfer.target == null)
            {
                CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, currentOperationId.ToString());
                return WebHookRequestFactory.Prompt(prompt);
            }
            else
            {
                CacheManager.Add(webHookGetEvents.uuid, CacheManager.CacheItemType.ParentID, currentOperationId.ToString()); // null verilebilir görüşme sonu 
                return WebHookRequestFactory.Transfer(transfer);
            }
        }
    }
}
