//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Verimor.Webhook.EventListener.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SubscriptionTelekomInfo
    {
        public long SubscriptionID { get; set; }
        public string SubscriptionNo { get; set; }
        public long TTCustomerCode { get; set; }
        public string PSTN { get; set; }
        public string RedbackName { get; set; }
        public short XDSLType { get; set; }
        public int PacketCode { get; set; }
        public int TariffCode { get; set; }
        public bool IsPaperWorkNeeded { get; set; }
    
        public virtual Subscription Subscription { get; set; }
    }
}
