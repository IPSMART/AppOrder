namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_SDDTL")]
    public partial class M_SUBLEG_SDDTL
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
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(4)]
        public string TAXGRPCD { get; set; }

        [StringLength(8)]
        public string TRSLCD { get; set; }

        public double? CRLIMIT { get; set; }

        public short? CRDAYS { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }
        [StringLength(8)]
        public string SLMSLCD { get; set; }
    }
}
