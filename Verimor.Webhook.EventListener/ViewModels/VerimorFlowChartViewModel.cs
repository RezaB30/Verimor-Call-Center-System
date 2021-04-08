using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener.ViewModels
{
    public class VerimorFlowChartViewModel
    {
        public int? ParentId { get; set; }
        public IEnumerable<FlowChartItems> FlowChartItemList { get; set; }
        public class FlowChartItems
        {
            public string Digit { get; set; }
            public int? OperationId { get; set; }
        }
    }    
}