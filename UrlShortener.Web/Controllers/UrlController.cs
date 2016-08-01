using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using UrlShortener.Business;
using UrlShortener.Entities;
using UrlShortener.Exceptions;
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
                try
                {
                    ShortUrl shortUrl = await _urlManager.ShortenUrl(url.LongUrl, Request.UserHostAddress);
                    url.ShortUrl = GetShortUrl(shortUrl.Segment);
                }
                catch (NotExistingUrlException)
                {
                    ModelState.AddModelError("LongUrl", "Url doesn't exist");
                }
            }
            return View(url);
        }

        public async Task<ActionResult> Click(string segment) {
            // http:// should preceed .LongUrl for proper redirect
            string referer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty;
            Statistics stat = await this._urlManager.Click(segment, referer, Request.UserHostAddress);
            //return RedirectPermanent(stat.ShortUrl.LongUrl);
            return Redirect(stat.ShortUrl.LongUrl);
        }

        public async Task<ActionResult> List()
        {
            var list = await _urlManager.EnumUrls();
            var result = list.Select(url => new Url{LongUrl = url.LongUrl, ShortUrl = GetShortUrl(url.Segment), CreatedDate = url.Added, CreatedIp = url.Ip, NumOfClicks = url.NumOfClicks});
            return View("UrlList", result);
        }

        private string GetShortUrl(string segment)
        {
            return string.Format("{0}://{1}{2}{3}", Request.Url.Scheme, Request.Url.Authority,
                        Url.Content("~"), segment);
        }
    }
}
