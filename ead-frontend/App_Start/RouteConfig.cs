using System.Web.Mvc;
using System.Web.Routing;

namespace ead_frontend
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "Guide Preview Route",
                url: "findingaids/preview",
                defaults: new { controller = "Guide", action = "Preview" }
            );

            routes.MapRoute(
                name: "Guide Request",
                url: "findingaids/{eadid}/request",
                defaults: new { controller = "Guide", action = "Pass" }
            );

            routes.MapRoute(
                name: "Guide Route",
                url: "findingaids/{eadid}",
                defaults: new { controller = "Guide", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
