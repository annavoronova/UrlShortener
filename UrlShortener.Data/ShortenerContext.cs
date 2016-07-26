using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrlShortener.Entities;

namespace UrlShortener.Data
{
    //[DbConfigurationType(typeof())]
    public class ShortenerContext : DbContext {
        public ShortenerContext()
            : base("name=ShortenerConnection") {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Statistics>()
                .HasRequired(s => s.ShortUrl)
                .WithMany(u => u.Statistics)
                .Map(m => m.MapKey("shortUrl_id"));
        }

        public virtual DbSet<ShortUrl> ShortUrls { get; set; }
        public virtual DbSet<Statistics> Statistics { get; set; }
    }
}
