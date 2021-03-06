namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNTRANS")]
    public partial class T_TXNTRANS
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(8)]
        public string TRANSLCD { get; set; }

        [StringLength(20)]
        public string EWAYBILLNO { get; set; }

        [StringLength(15)]
        public string LRNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LRDT { get; set; }

        [StringLength(50)]
        public string LORRYNO { get; set; }

        public double? GRWT { get; set; }

        public double? TRWT { get; set; }

        public double? NTWT { get; set; }

        [StringLength(2)]
        public string TRANSMODE { get; set; }

        [StringLength(1)]
        public string VECHLTYPE { get; set; }
        [StringLength(8)]
        public string CRSLCD { get; set; }

    }
}
