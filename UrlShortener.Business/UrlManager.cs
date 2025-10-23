using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using UrlShortener.Data;
using UrlShortener.Entities;
using UrlShortener.Exceptions;

namespace UrlShortener.Business
{
    public class UrlManager : IUrlManager
    {
        private IShortenerContext ctx;

        public UrlManager(IShortenerContext ctx)
        {
            this.ctx = ctx;
        }

        public Task<List<Entities.ShortUrl>> EnumUrls()
        {
            return Task.Run(() =>
            {
                Log.Information("UrlManager.EnumUrls - Retrieving all URLs");
                
                using (var ctx = new ShortenerContext())
                {
                    var urls = ctx.ShortUrls.OrderBy(x => x.LongUrl).ToList();
                    Log.Information("UrlManager.EnumUrls - Retrieved {Count} URLs", urls.Count);
                    return urls;
                }
            });
        }

        public Task<ShortUrl> ShortenUrl(string longUrl, string ip, string segment = "")
        {
            return Task.Run(() =>
            {
                Log.Information("UrlManager.ShortenUrl - Starting URL shortening for: {LongUrl}, IP: {IP}, CustomSegment: {Segment}", 
                    longUrl, ip, segment);
                
                // Input validation
                if (string.IsNullOrWhiteSpace(longUrl))
                {
                    Log.Warning("UrlManager.ShortenUrl - Empty URL provided");
                    throw new ArgumentException("URL cannot be null or empty", nameof(longUrl));
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    Log.Warning("UrlManager.ShortenUrl - Empty IP provided");
                    throw new ArgumentException("IP address cannot be null or empty", nameof(ip));
                }

                using (var ctx = new ShortenerContext())
                {
                    // Normalize URL with scheme for consistent comparison
                    var normalizedUrl = GetUrlWithScheme(longUrl);
                    Log.Debug("UrlManager.ShortenUrl - Normalized URL: {NormalizedUrl}", normalizedUrl);
                    
                    var url = ctx.ShortUrls.FirstOrDefault(u => u.LongUrl == normalizedUrl);
                    if (url != null)
                    {
                        Log.Information("UrlManager.ShortenUrl - Found existing URL: {LongUrl} -> {Segment}", 
                            normalizedUrl, url.Segment);
                        return url;
                    }

                    if (!string.IsNullOrEmpty(segment))
                    {
                        if (ctx.ShortUrls.Any(u => u.Segment == segment))
                        {
                            Log.Warning("UrlManager.ShortenUrl - Duplicate segment: {Segment}", segment);
                            throw new DuplicatedSegmentException();
                        }
                        Log.Information("UrlManager.ShortenUrl - Using custom segment: {Segment}", segment);
                    } 
                    else 
                    {
                        Log.Debug("UrlManager.ShortenUrl - Validating URL accessibility");
                        CheckIfUrlValid(normalizedUrl);
                        segment = GenerateUniqueSegment(ctx);
                        Log.Information("UrlManager.ShortenUrl - Generated new segment: {Segment}", segment);
                    }

                    if (string.IsNullOrEmpty(segment))
                    {
                        Log.Error("UrlManager.ShortenUrl - Failed to generate unique segment");
                        throw new InvalidOperationException("Failed to generate unique segment");
                    }

                    url = new ShortUrl()
                    {
                        Added = DateTime.UtcNow,
                        Ip = ip,
                        LongUrl = normalizedUrl,
                        NumOfClicks = 0,
                        Segment = segment
                    };

                    try
                    {
                        ctx.ShortUrls.Add(url);
                        ctx.SaveChanges();
                        
                        Log.Information("UrlManager.ShortenUrl - Successfully created short URL: {LongUrl} -> {Segment}", 
                            normalizedUrl, segment);
                        return url;
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        // Handle race condition - segment might have been created by another thread
                        if (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                            ex.InnerException?.Message.Contains("duplicate") == true)
                        {
                            Log.Warning(ex, "UrlManager.ShortenUrl - Race condition detected for segment: {Segment}", segment);
                            throw new DuplicatedSegmentException("Segment already exists", ex);
                        }
                        Log.Error(ex, "UrlManager.ShortenUrl - Database error saving URL: {LongUrl}", normalizedUrl);
                        throw;
                    }
                }
            });
        }

        private static string GetUrlWithScheme(string url)
        {
            if (!url.StartsWith(Uri.UriSchemeHttp + "://") && !url.StartsWith(Uri.UriSchemeHttps + "://"))
            {
                url = Uri.UriSchemeHttp + "://" + url;
            }
            return url;
        }

        private static void CheckIfUrlValid(string longUrl)
        {
            Log.Debug("UrlManager.CheckIfUrlValid - Validating URL: {LongUrl}", longUrl);
            
            Uri urlCheck = new Uri(longUrl);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(urlCheck);
            request.Timeout = 10000;
            request.Method = "HEAD"; // Use HEAD request to avoid downloading content
            
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Log.Debug("UrlManager.CheckIfUrlValid - URL validation successful: {LongUrl}, Status: {StatusCode}", 
                        longUrl, response.StatusCode);
                }
            }
            catch (WebException ex)
            {
                Log.Warning(ex, "UrlManager.CheckIfUrlValid - URL validation failed: {LongUrl}, Status: {Status}", 
                    longUrl, ex.Status);
                // Handle specific web exceptions
                if (ex.Status == WebExceptionStatus.Timeout || 
                    ex.Status == WebExceptionStatus.ConnectFailure ||
                    ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    throw new NotExistingUrlException($"URL is not accessible: {ex.Message}", ex);
                }
                throw new NotExistingUrlException($"URL validation failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UrlManager.CheckIfUrlValid - Unexpected error validating URL: {LongUrl}", longUrl);
                throw new NotExistingUrlException($"URL validation failed: {ex.Message}", ex);
            }
        }

        public Task<Statistics> Click(string segment, string referer, string ip)
        {
            return Task.Run(() =>
            {
                Log.Information("UrlManager.Click - Processing click for segment: {Segment}, IP: {IP}, Referer: {Referer}", 
                    segment, ip, referer);
                
                // Input validation
                if (string.IsNullOrWhiteSpace(segment))
                {
                    Log.Warning("UrlManager.Click - Empty segment provided");
                    throw new ArgumentException("Segment cannot be null or empty", nameof(segment));
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    Log.Warning("UrlManager.Click - Empty IP provided");
                    throw new ArgumentException("IP address cannot be null or empty", nameof(ip));
                }

                using (var ctx = new ShortenerContext())
                {
                    ShortUrl url = ctx.ShortUrls.FirstOrDefault(u => u.Segment == segment);
                    if (url == null)
                    {
                        Log.Warning("UrlManager.Click - Short URL not found for segment: {Segment}", segment);
                        throw new NotFoundShortUrlException();
                    }

                    Log.Debug("UrlManager.Click - Found URL for segment: {Segment} -> {LongUrl}", segment, url.LongUrl);

                    url.NumOfClicks = url.NumOfClicks + 1;

                    Statistics stat = new Statistics()
                    {
                        ClickDate = DateTime.UtcNow,
                        Ip = ip,
                        Referrer = referer ?? string.Empty,
                        ShortUrl = url
                    };

                    ctx.Statistics.Add(stat);
                    ctx.SaveChanges();

                    Log.Information("UrlManager.Click - Click processed successfully for segment: {Segment}, Total clicks: {TotalClicks}", 
                        segment, url.NumOfClicks);

                    return stat;
                }
            });
        }

        private string GenerateUniqueSegment(IShortenerContext ctx)
        {
            Log.Debug("UrlManager.GenerateUniqueSegment - Starting segment generation");
            
            int attempts = 0;
            const int maxAttempts = 50;
            
            while (attempts < maxAttempts)
            {
                string segment = Guid.NewGuid().ToString().Substring(0, Configurator.SegmentLength);
                if (!ctx.ShortUrls.Any(u => u.Segment == segment))
                {
                    Log.Debug("UrlManager.GenerateUniqueSegment - Generated unique segment: {Segment} after {Attempts} attempts", 
                        segment, attempts + 1);
                    return segment;
                }
                attempts++;
            }
            
            Log.Error("UrlManager.GenerateUniqueSegment - Failed to generate unique segment after {MaxAttempts} attempts", maxAttempts);
            throw new InvalidOperationException($"Failed to generate unique segment after {maxAttempts} attempts");
        }
    }
}
