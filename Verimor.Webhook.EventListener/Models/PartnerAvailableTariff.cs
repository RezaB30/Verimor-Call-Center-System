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
    
    public partial class PartnerAvailableTariff
    {
        public long ID { get; set; }
        public int PartnerGroupID { get; set; }
        public int TariffID { get; set; }
        public Nullable<short> Commitment { get; set; }
        public decimal Allowance { get; set; }
        public int DomainID { get; set; }
        public decimal AllowanceThreshold { get; set; }
    
        public virtual Domain Domain { get; set; }
        public virtual PartnerGroup PartnerGroup { get; set; }
        public virtual Service Service { get; set; }
    }
}
