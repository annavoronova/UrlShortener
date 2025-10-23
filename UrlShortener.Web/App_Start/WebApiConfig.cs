using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Swashbuckle.Application;

namespace UrlShortener.Web {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure Swagger
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "UrlShortener API")
                 .Description("A simple URL shortener service API")
                 .Contact(cc => cc
                     .Name("UrlShortener Team")
                     .Email("support@urlshortener.com"))
                 .License(lc => lc
                     .Name("MIT License")
                     .Url("https://opensource.org/licenses/MIT"));
            })
            .EnableSwaggerUi(c =>
            {
                c.DocumentTitle("UrlShortener API Documentation");
                c.DocExpansion(DocExpansion.List);
            });

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}