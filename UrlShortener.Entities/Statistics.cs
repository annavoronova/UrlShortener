using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Entities
{
    [Table("tbl_statistics")]
    public class Statistics {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("click_date")]
        public DateTime ClickDate { get; set; }

        [Required]
        [Column("ip")]
        [StringLength(50)]
        public string Ip { get; set; }

        [Column("referrer")]
        [StringLength(500)]
        public string Referrer { get; set; }

        public ShortUrl ShortUrl { get; set; }
    }
}
