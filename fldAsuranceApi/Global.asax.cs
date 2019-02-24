using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace fldAsuranceApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            var context = HttpContext.Current;
            var response = context.Response;

            // enable CORS
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("X-Frame-Options", "ALLOW-FROM *");

            if (context.Request.HttpMethod == "OPTIONS")
            {
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, Paging-Headers, Date, Content-Length, AuthResponse-Headers, Authorization, App");
                response.AddHeader("Access-Control-Expose-Headers", "Server-Authorization, Paging-Headers, Date, Content-Length, AuthResponse-Headers");
                response.AddHeader("Access-Control-Max-Age", "1728000");
                response.End();
            }
        }
    }
}
