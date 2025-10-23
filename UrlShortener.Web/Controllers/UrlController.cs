using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Serilog;
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
            Log.Information("UrlController.Index GET - User accessing main page");
            Url url = new Url();
            return View(url);
        }

        public async Task<ActionResult> Index(Url url) {
            Log.Information("UrlController.Index POST - User submitted URL: {LongUrl}", url?.LongUrl);
            
            if (ModelState.IsValid)
            {
                try
                {
                    ShortUrl shortUrl = await _urlManager.ShortenUrl(url.LongUrl, Request.UserHostAddress);
                    url.ShortUrl = GetShortUrl(shortUrl.Segment);
                    Log.Information("URL shortened successfully: {LongUrl} -> {ShortUrl}", url.LongUrl, url.ShortUrl);
                }
                catch (NotExistingUrlException)
                {
                    Log.Warning("URL does not exist: {LongUrl}", url.LongUrl);
                    ModelState.AddModelError("LongUrl", "Url doesn't exist");
                }
                catch (DuplicatedSegmentException)
                {
                    Log.Warning("Duplicate segment for URL: {LongUrl}", url.LongUrl);
                    ModelState.AddModelError("LongUrl", "This URL segment is already taken");
                }
                catch (ArgumentException ex)
                {
                    Log.Warning(ex, "Invalid argument for URL: {LongUrl}", url.LongUrl);
                    ModelState.AddModelError("LongUrl", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error shortening URL: {LongUrl}", url.LongUrl);
                    ModelState.AddModelError("LongUrl", "An error occurred while processing your request");
                    // Log the exception for debugging
                    System.Diagnostics.Debug.WriteLine($"Error in Index: {ex}");
                }
            }
            else
            {
                Log.Warning("Model state is invalid for URL: {LongUrl}", url?.LongUrl);
            }
            
            return View(url);
        }

        public async Task<ActionResult> Click(string segment) {
            Log.Information("UrlController.Click - User clicked segment: {Segment}", segment);
            
            try
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    Log.Warning("Empty segment provided for click");
                    return RedirectToAction("Index");
                }

                string referer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty;
                Statistics stat = await this._urlManager.Click(segment, referer, Request.UserHostAddress);
                
                Log.Information("Click processed successfully for segment: {Segment}, redirecting to: {LongUrl}", 
                    segment, stat.ShortUrl.LongUrl);
                
                return Redirect(stat.ShortUrl.LongUrl);
            }
            catch (NotFoundShortUrlException)
            {
                Log.Warning("Short URL not found for segment: {Segment}", segment);
                // Redirect to home page with error message
                TempData["ErrorMessage"] = "The requested short URL was not found.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Invalid argument for click segment: {Segment}", segment);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing click for segment: {Segment}", segment);
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error in Click: {ex}");
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult List()
        {
            Log.Information("UrlController.List - User accessing URL list page");
            return View("UrlList");
        }

        public async Task<JsonResult> ListUrls()
        {
            Log.Information("UrlController.ListUrls - User requesting URL list via AJAX");
            
            try
            {
                var list = await _urlManager.EnumUrls();
                var result = list.Select(url => new Url { LongUrl = url.LongUrl, ShortUrl = GetShortUrl(url.Segment), CreatedDate = url.Added, CreatedIp = url.Ip, NumOfClicks = url.NumOfClicks });
                
                Log.Information("Retrieved {Count} URLs for list", list.Count);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving URL list");
                return Json(new { error = "Failed to retrieve URLs" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetShortUrl(string segment)
        {
            return string.Format("{0}://{1}{2}{3}", Request.Url.Scheme, Request.Url.Authority,
                        Request.ApplicationPath + "/", segment);
        }
    }
}
