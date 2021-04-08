using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener.ViewModels
{
    public class VerimorOperationsViewModel
    {
        public int ID { get; set; }
        [DisplayName("İşlem Tipi")]
        public int? OperationType { get; set; }
        [DisplayName("Başlık")]
        public string Title { get; set; }
    }
}