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
    
    public partial class VerimorOperationResponse
    {
        public int ID { get; set; }
        public int OperationID { get; set; }
        public string digit { get; set; }
        public Nullable<int> ParentID { get; set; }
    
        public virtual VerimorOperation VerimorOperation { get; set; }
    }
}
