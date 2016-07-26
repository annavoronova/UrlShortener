using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UrlShortener.Web.Models {
    public class Url {
        [Required]
        [StringLength(1000)]
        [DisplayName("URL")]
        public string LongUrl { get; set; }
        [DisplayName("Shorten URL")]
        public string ShortUrl { get; set; }
        [DisplayName("Created")]
        public DateTime CreatedDate { get; set; }
        [DisplayName("IP")]
        public string CreatedIp { get; set; }
        [DisplayName("# of clicks")]
        public int NumOfClicks { get; set; }
    }
}