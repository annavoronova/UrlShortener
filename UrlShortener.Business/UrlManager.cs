using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                using (var ctx = new ShortenerContext())
                {
                    return ctx.ShortUrls.OrderBy(x => x.LongUrl).ToList();
                }
            });
        }

        public Task<ShortUrl> ShortenUrl(string longUrl, string ip, string segment = "")
        {
            return Task.Run(() =>
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(longUrl))
                {
                    throw new ArgumentException("URL cannot be null or empty", nameof(longUrl));
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    throw new ArgumentException("IP address cannot be null or empty", nameof(ip));
                }

                using (var ctx = new ShortenerContext())
                {
                    // Normalize URL with scheme for consistent comparison
                    var normalizedUrl = GetUrlWithScheme(longUrl);
                    
                    var url = ctx.ShortUrls.FirstOrDefault(u => u.LongUrl == normalizedUrl);
                    if (url != null)
                    {
                        return url;
                    }

                    if (!string.IsNullOrEmpty(segment))
                    {
                        if (ctx.ShortUrls.Any(u => u.Segment == segment))
                        {
                            throw new DuplicatedSegmentException();
                        }
                    } 
                    else 
                    {
                        CheckIfUrlValid(normalizedUrl);
                        segment = GenerateUniqueSegment(ctx);
                    }

                    if (string.IsNullOrEmpty(segment))
                    {
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
                        return url;
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                    {
                        // Handle race condition - segment might have been created by another thread
                        if (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                            ex.InnerException?.Message.Contains("duplicate") == true)
                        {
                            throw new DuplicatedSegmentException("Segment already exists", ex);
                        }
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
            Uri urlCheck = new Uri(longUrl);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(urlCheck);
            request.Timeout = 10000;
            request.Method = "HEAD"; // Use HEAD request to avoid downloading content
            
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    // Response is automatically disposed by the using statement
                }
            }
            catch (WebException ex)
            {
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
                throw new NotExistingUrlException($"URL validation failed: {ex.Message}", ex);
            }
        }

        public Task<Statistics> Click(string segment, string referer, string ip)
        {
            return Task.Run(() =>
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(segment))
                {
                    throw new ArgumentException("Segment cannot be null or empty", nameof(segment));
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    throw new ArgumentException("IP address cannot be null or empty", nameof(ip));
                }

                using (var ctx = new ShortenerContext())
                {
                    ShortUrl url = ctx.ShortUrls.FirstOrDefault(u => u.Segment == segment);
                    if (url == null)
                    {
                        throw new NotFoundShortUrlException();
                    }

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

                    return stat;
                }
            });
        }

        private string GenerateUniqueSegment(IShortenerContext ctx)
        {
            int attempts = 0;
            const int maxAttempts = 50;
            
            while (attempts < maxAttempts)
            {
                string segment = Guid.NewGuid().ToString().Substring(0, Configurator.SegmentLength);
                if (!ctx.ShortUrls.Any(u => u.Segment == segment))
                {
                    return segment;
                }
                attempts++;
            }
            
            throw new InvalidOperationException($"Failed to generate unique segment after {maxAttempts} attempts");
        }
    }
}
