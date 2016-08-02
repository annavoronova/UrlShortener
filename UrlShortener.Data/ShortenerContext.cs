using System.Data.Entity;
using UrlShortener.Entities;

namespace UrlShortener.Data
{
    public class ShortenerContext : DbContext, IShortenerContext {
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
