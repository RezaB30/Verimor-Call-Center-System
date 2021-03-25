using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHook
{
    public class WebHookEvents
    {
        public WebHookResponse Prompt(Prompt events)
        {
            return new WebHookResponse() { prompt = events };
        }
        public WebHookResponse Transfer(Transfer events)
        {
            return new WebHookResponse() { transfer = events };
        }
        public WebHookResponse Record(Record events)
        {
            return new WebHookResponse() { record = events };
        }
    }
}
