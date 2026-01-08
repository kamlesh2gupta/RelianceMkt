using System.Web.Http;

namespace RelianceMkt.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // ✅ MUST BE FIRST LINE
            config.MapHttpAttributeRoutes();

            // Conventional routing (backup)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}