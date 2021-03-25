using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener.ViewModels
{
    public class VerimorSettings
    {
        public int ID { get; set; }
        //public VerimorAnnounceTypes Parent { get; set; }
        //public VerimorAnnounceTypes Child { get; set; }
        [DisplayName("Üst Menü")]
        public int? Parent { get; set; }
        [DisplayName("Alt Menü")]
        public int Child { get; set; }
        [DisplayName("Tuş")]
        public string Digit { get; set; }
        [DisplayName("Üst Menü")]
        public int ParentList { get; set; }
        [DisplayName("Alt Menü")]
        public int ChildList { get; set; }
        [DisplayName("Tuş")]
        public string DigitList { get; set; }
        [DisplayName("Seçili Üst Menü")]
        public int? SelectedParent { get; set; }
        [DisplayName("Seçili Alt Menü")]
        public int? SelectedChild { get; set; }

    }
}