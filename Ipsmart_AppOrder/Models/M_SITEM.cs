namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEM")]
    public partial class M_SITEM
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
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }

        [StringLength(20)]
        public string SHORTNM { get; set; }

        [StringLength(20)]
        public string PRODCD { get; set; }

        [Required]
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [Required]
        [StringLength(4)]
        public string UOMCD { get; set; }

        [StringLength(8)]
        public string HSNSACCD { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double? PACKSIZE { get; set; }

        public short? NOSINBAG { get; set; }

        public double? MARGINPER { get; set; }
        [Required]
        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        [StringLength(1)]
        public string STKNOTEFFECT { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(4)]
        public string BRGRPCD { get; set; }
        [StringLength(15)]
        public string PROP1 { get; set; }
        [StringLength(30)]
        public string PROP2 { get; set; }
        [StringLength(15)]
        public string PROP3 { get; set; }
        [StringLength(30)]
        public string PROP4 { get; set; }
        [StringLength(30)]
        public string PROP5 { get; set; }
        [StringLength(30)]
        public string PROP6 { get; set; }
        [StringLength(30)]
        public string LINKITCD { get; set; }
    }
}
