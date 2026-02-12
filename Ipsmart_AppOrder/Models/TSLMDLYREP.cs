using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSLMDLYREP
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte SLNO { get; set; }

        [StringLength(8)]
        public string DISTSLCD { get; set; }

        [StringLength(8)]
        public string RTLCD { get; set; }

        [StringLength(4)]
        public string BRANDCD { get; set; }

        [Required]
        [StringLength(12)]
        public string ITMCTG { get; set; }

        [Required]
        [StringLength(500)]
        public string DTLS { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? AMT { get; set; }

        //[DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
    }
}