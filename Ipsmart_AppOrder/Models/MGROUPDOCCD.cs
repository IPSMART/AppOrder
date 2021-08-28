using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MGROUPDOCCD
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
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string DOCCD { get; set; }
        public byte SLNO { get; set; }
        public bool Checked { get; set; }

        [StringLength(35)]
        public string DOCNM { get; set; }

    }
}