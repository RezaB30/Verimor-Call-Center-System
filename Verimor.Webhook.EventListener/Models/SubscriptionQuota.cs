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
    
    public partial class SubscriptionQuota
    {
        public long ID { get; set; }
        public long SubscriptionID { get; set; }
        public System.DateTime AddDate { get; set; }
        public long Amount { get; set; }
    
        public virtual Subscription Subscription { get; set; }
    }
}
