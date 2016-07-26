using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using UrlShortener.Business;
using UrlShortener.Entities;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    public class UrlController : Controller
    {
        private IUrlManager _urlManager;

        public UrlController(IUrlManager urlManager) {
            this._urlManager = urlManager;
        }

        [HttpGet]
        public ActionResult Index() {
            Url url = new Url();
            return View(url);
        }

        public async Task<ActionResult> Index(Url url) {
            if (ModelState.IsValid)
            {
               ShortUrl shortUrl = await _urlManager.ShortenUrl(url.LongUrl, Request.UserHostAddress);
               url.ShortUrl = string.Format("{0}://{1}{2}{3}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"), shortUrl.Segment);
            }
            return View(url);
        }

        public async Task<ActionResult> Click(string segment) {
            // http:// should preceed .LongUrl for proper redirect
            string referer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty;
            Statistics stat = await this._urlManager.Click(segment, referer, Request.UserHostAddress);
            return RedirectPermanent(stat.ShortUrl.LongUrl);
        }

        [HttpGet]
        public ActionResult List()
        {
            var list = new List<Url>();
            return View("UrlList", list);
        }
    }
}
