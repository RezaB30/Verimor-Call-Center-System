using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener.ViewModels
{
    public class GeneralFaultViewModel
    {
        public long ID { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string ProvinceName { get; set; }
        public long ProvinceId { get; set; }
    }
}