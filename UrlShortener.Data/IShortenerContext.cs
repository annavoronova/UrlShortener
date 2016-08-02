using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener.Entities;

namespace UrlShortener.Data
{
    public interface IShortenerContext {
        DbSet<ShortUrl> ShortUrls { get; set; }
        DbSet<Statistics> Statistics { get; set; }
    }
}
