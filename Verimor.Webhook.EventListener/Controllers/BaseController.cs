using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class BaseController : Controller
    {
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("tr-tr");
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("tr-tr");
            return base.BeginExecuteCore(callback, state);
        }
    }
}