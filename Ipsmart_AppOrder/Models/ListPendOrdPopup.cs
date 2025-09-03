using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class ListPendOrdPopup
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }
        public string SIZEDET { get; set; }
        public string ALLSIZES { get; set; }


        [StringLength(1)]
        public string FREESTK { get; set; }

        public double QNTY { get; set; }

        public double TRTLQNTY { get; set; }

        public double? TSTKQNTY { get; set; }
        public string STYLENO { get; set; }
        public string RTLAUTONO { get; set; }
        public double TRTLBOX { get; set; }
        public double SET { get; set; }
        [StringLength(50)]
        public string ORDSKIPREASON { get; set; }
        [StringLength(50)]
        public string ITREM { get; set; }
        public int ParentSerialNo { get; set; }
        public double PCSPERSET { get; set; }
        public double PCSPERBOX { get; set; }
        public double TRTLSET { get; set; }
        public string MIXSIZE { get; set; }
        public double SIZE_COUNT { get; set; }
        public string CheckedORDSKIP { get; set; }
    }
}