using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadiusR.Verimor.CallCenter
{
    public static class WebHookRequestFactory
    {
        public static WebHookRequest Prompt(WebHookRequest.Prompt events)
        {
            return new WebHookRequest() { prompt = events };
        }
        public static WebHookRequest Transfer(WebHookRequest.Transfer events)
        {
            return new WebHookRequest() { transfer = events };
        }
        public static WebHookRequest Record(WebHookRequest.Record events)
        {
            return new WebHookRequest() { record = events };
        }
    }
}
