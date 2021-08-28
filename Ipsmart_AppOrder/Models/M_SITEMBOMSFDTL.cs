namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEMBOMSFDTL")]
    public partial class M_SITEMBOMSFDTL
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
        [StringLength(10)]
        public string BOMCD { get; set; }

        public short? SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string ITCD { get; set; }

        public double? QNTY { get; set; }

        public double? RATE { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }
        
    }
}
