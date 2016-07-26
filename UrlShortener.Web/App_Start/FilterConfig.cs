using System.Web;
using System.Web.Mvc;
using UrlShortener.Web.Filters;

namespace UrlShortener.Web {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new ShortenerErrorFilter());
        }
    }
}