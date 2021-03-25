using System.Web;
using System.Web.Mvc;

namespace Verimor.Webhook.EventListener
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
