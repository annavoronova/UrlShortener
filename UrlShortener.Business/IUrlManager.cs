using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener.Entities;

namespace UrlShortener.Business
{
    public interface IUrlManager
    {
        Task<List<Entities.ShortUrl>> EnumUrls();
        Task<ShortUrl> ShortenUrl(string longUrl, string ip, string segment = "");
        Task<Statistics> Click(string segment, string referer, string ip);
    }
}
