using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.Verimor.CallCenter.Caching
{
    public static class CacheManager
    {
        private static MemoryCache _cache = new MemoryCache("WebhookCache");

        public static void Add(string key, CacheItemType type, string value)
        {
            _cache.Set(GetCacheKey(key, type), value, DateTimeOffset.Now.AddMinutes(15));
        }

        public static string Get(string key, CacheItemType type)
        {
            return _cache.Get(GetCacheKey(key, type)) as string;
        }

        private static string GetCacheKey(string key, CacheItemType type)
        {
            return string.Format("{0}_{1}", key, Enum.GetName(typeof(CacheItemType), type));
        }

        public enum CacheItemType
        {
            SubscriberNo,
            SubscriberName,
            PhoneNo,
            UnpaidBillCount,
            TCK,
            PaymentCardNo,
            PaymentCardExpDate,
            PaymentCardCVV,
            DebtAmount,
            SubscriptionID,
            WrongDialing,
            ParentID,
            SubscriptionList,
            GeneralFault,
            UnpaidBillSubscriptionCount,
            CancelledSubscriptions,
            PaymentCardToken,
            WrongDialingMaxDigits,
            WrongDialingMinDigits,
            UnpaidBillList,
            ValidationSMS,
            CompanyGeneralFault
        }
    }
}
