using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Serilog;
using UrlShortener.Business;
using UrlShortener.Entities;
using UrlShortener.Exceptions;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    /// <summary>
    /// API controller for URL shortening operations
    /// </summary>
    [RoutePrefix("api/urls")]
    public class UrlApiController : ApiController
    {
        private readonly IUrlManager _urlManager;

        public UrlApiController(IUrlManager urlManager)
        {
            _urlManager = urlManager;
        }

        /// <summary>
        /// Get all shortened URLs
        /// </summary>
        /// <returns>List of all shortened URLs</returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAllUrls()
        {
            try
            {
                Log.Information("Getting all URLs");
                var urls = await _urlManager.EnumUrls();
                var result = urls.Select(url => new Url 
                { 
                    LongUrl = url.LongUrl, 
                    ShortUrl = GetShortUrl(url.Segment), 
                    CreatedDate = url.Added, 
                    CreatedIp = url.Ip, 
                    NumOfClicks = url.NumOfClicks 
                });
                
                Log.Information("Retrieved {Count} URLs", urls.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all URLs");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Shorten a URL
        /// </summary>
        /// <param name="request">URL shortening request</param>
        /// <returns>Shortened URL information</returns>
        [HttpPost]
        [Route("shorten")]
        public async Task<IHttpActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.LongUrl))
                {
                    Log.Warning("Invalid shorten URL request received");
                    return BadRequest("LongUrl is required");
                }

                Log.Information("Shortening URL: {LongUrl}", request.LongUrl);
                var clientIp = GetClientIpAddress();
                
                var shortUrl = await _urlManager.ShortenUrl(
                    request.LongUrl, 
                    clientIp, 
                    request.CustomSegment);

                var result = new Url
                {
                    LongUrl = shortUrl.LongUrl,
                    ShortUrl = GetShortUrl(shortUrl.Segment),
                    CreatedDate = shortUrl.Added,
                    CreatedIp = shortUrl.Ip,
                    NumOfClicks = shortUrl.NumOfClicks
                };

                Log.Information("URL shortened successfully: {LongUrl} -> {ShortUrl}", 
                    request.LongUrl, result.ShortUrl);
                
                return Ok(result);
            }
            catch (NotExistingUrlException ex)
            {
                Log.Warning(ex, "URL does not exist: {LongUrl}", request?.LongUrl);
                return BadRequest("The provided URL does not exist or is not accessible");
            }
            catch (DuplicatedSegmentException ex)
            {
                Log.Warning(ex, "Duplicate segment: {Segment}", request?.CustomSegment);
                return Conflict("The custom segment is already taken");
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Invalid argument: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error shortening URL: {LongUrl}", request?.LongUrl);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get URL statistics by segment
        /// </summary>
        /// <param name="segment">URL segment</param>
        /// <returns>URL statistics</returns>
        [HttpGet]
        [Route("{segment}/stats")]
        public async Task<IHttpActionResult> GetUrlStats(string segment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    return BadRequest("Segment is required");
                }

                Log.Information("Getting stats for segment: {Segment}", segment);
                
                // This would need to be implemented in the business layer
                // For now, return a placeholder response
                var stats = new
                {
                    Segment = segment,
                    TotalClicks = 0,
                    LastClick = (DateTime?)null,
                    CreatedDate = (DateTime?)null
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting stats for segment: {Segment}", segment);
                return InternalServerError(ex);
            }
        }

        private string GetShortUrl(string segment)
        {
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            return $"{baseUrl}/{segment}";
        }

        private string GetClientIpAddress()
        {
            var request = HttpContext.Current?.Request;
            if (request == null) return "unknown";

            var ipAddress = request.Headers["X-Forwarded-For"] ??
                           request.Headers["X-Real-IP"] ??
                           request.UserHostAddress;

            return ipAddress ?? "unknown";
        }
    }
}
