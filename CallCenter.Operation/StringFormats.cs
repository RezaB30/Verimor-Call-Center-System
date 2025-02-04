﻿using RadiusR.Verimor.CallCenter;
using RadiusR.Verimor.CallCenter.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenter.Operation
{
    public static class StringFormats
    {
        public static string GetFormats(string phrase, WebHookResponse webHookGetEvents)
        {
            if (phrase != null)
            {
                var Result = phrase.Replace("{Name}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.SubscriberName));
                Result = Result.Replace("{DebtAmount}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.DebtAmount));
                Result = Result.Replace("{UnpaidBillCount}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.UnpaidBillCount));
                Result = Result.Replace("{CreditCardNo}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.PaymentCardNo));
                Result = Result.Replace("{GeneralFault}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.GeneralFault));
                Result = Result.Replace("{CompanyGeneralFault}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.CompanyGeneralFault));
                Result = Result.Replace("{CompanyGeneralFaultStartTime}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.CompanyGeneralFaultStartTime));
                Result = Result.Replace("{CompanyGeneralFaultEndTime}", CacheManager.Get(webHookGetEvents.uuid, CacheManager.CacheItemType.CompanyGeneralFaultEndTime));
                return Result;
            }
            else
            {
                return "";
            }
        }
        public static int BooleanToInteger(bool? operationResult)
        {
            if (operationResult == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
    public enum PhraseTypes
    {
        Name,
        DebtAmount,
        UnpaidBillCount,
        CreditCardNo,
        GeneralFault,
        CompanyGeneralFault,
        CompanyGeneralFaultStartTime,
        CompanyGeneralFaultEndTime
    }
}
