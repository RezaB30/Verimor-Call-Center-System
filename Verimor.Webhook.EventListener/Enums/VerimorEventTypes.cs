using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verimor.Webhook.EventListener.Enums
{
    public enum VerimorEventTypes
    {
        ringing = 1,
        answer = 2,
        hangup = 3,
        user_hangup = 4
    }
}
