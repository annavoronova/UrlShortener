using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UrlShortener.Exceptions;

namespace UrlShortener.Web.Filters {
    public class ShortenerErrorFilter : HandleErrorAttribute {
        public override void OnException(ExceptionContext filterContext) {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            var ex = filterContext.Exception;
            string viewName = "Error500";

            if (ex is NotFoundShortUrlException) {
                code = HttpStatusCode.NotFound;
                viewName = "Error404";
            }
            if (ex is DuplicatedSegmentException) {
                code = HttpStatusCode.Conflict;
                viewName = "Error409";
            }

            filterContext.Result = new ViewResult() {
                ViewName = viewName
            };

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = (int)code;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}