﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Verimor.Webhook.EventListener.Binders;

namespace Verimor.Webhook.EventListener
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
            ModelBinders.Binders[typeof(DateTime?)] = new DateBinder();
            ModelBinders.Binders[typeof(DateTime)] = new DateBinder();
        }
    }
}
