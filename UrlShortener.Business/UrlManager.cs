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
                using (var ctx = new ShortenerContext())
                {
                    var url = ctx.ShortUrls.FirstOrDefault(u => u.LongUrl == longUrl);
                    if (url != null)
                    {
                        url.LongUrl = EnsureUrlHasScheme(url.LongUrl);
                        return url;
                    }

                    if (!string.IsNullOrEmpty(segment))
                    {
                        if (ctx.ShortUrls.Any(u => u.Segment == segment))
                        {
                            throw new DuplicatedSegmentException();
                        }
                    } else {
                        longUrl = EnsureUrlHasScheme(longUrl);

                        CheckIfUrlValid(longUrl);
                        segment = this.NewSegment();
                    }

                    if (string.IsNullOrEmpty(segment))
                    {
                        throw new ArgumentException("Segment is empty");
                    }

                    url = new ShortUrl()
                    {
                        Added = DateTime.Now,
                        Ip = ip,
                        LongUrl = longUrl,
                        NumOfClicks = 0,
                        Segment = segment
                    };

                    ctx.ShortUrls.Add(url);

                    ctx.SaveChanges();

                    return url;
                }
            });
        }

        private static string EnsureUrlHasScheme(string url)
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
            try
            {
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            }
            catch (Exception)
            {
                throw new NotExistingUrlException();
            }
        }

        public Task<Statistics> Click(string segment, string referer, string ip)
        {
            return Task.Run(() =>
            {
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
                        ClickDate = DateTime.Now,
                        Ip = ip,
                        Referrer = referer,
                        ShortUrl = url
                    };

                    ctx.Statistics.Add(stat);

                    ctx.SaveChanges();

                    return stat;
                }
            });
        }

        private string NewSegment()
        {
            using (var ctx = new ShortenerContext())
            {
                int i = 0;
                while (true)
                {
                    string segment = Guid.NewGuid().ToString().Substring(0, Configurator.SegmentLength);
                    if (!ctx.ShortUrls.Any(u => u.Segment == segment))
                    {
                        return segment;
                    }
                    if (i > 30)
                    {
                        break;
                    }
                    i++;
                }
                return string.Empty;
            }
        }
    }
}
