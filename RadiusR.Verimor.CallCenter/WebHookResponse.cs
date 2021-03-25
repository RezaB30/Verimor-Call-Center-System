using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.Verimor.CallCenter
{
    public class WebHookResponse
    {
        public string uuid { get; set; }
        public string cli { get; set; }
        public string cld { get; set; }
        public int step { get; set; }
        public string digits { get; set; }
        public string error { get; set; }
    }
}
