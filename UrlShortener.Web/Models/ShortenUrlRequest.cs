using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Models
{
    /// <summary>
    /// Request model for URL shortening
    /// </summary>
    public class ShortenUrlRequest
    {
        /// <summary>
        /// The URL to shorten
        /// </summary>
        [Required]
        [Url]
        [StringLength(1000)]
        public string LongUrl { get; set; }

        /// <summary>
        /// Optional custom segment for the short URL
        /// </summary>
        [StringLength(20)]
        public string CustomSegment { get; set; }
    }
}
