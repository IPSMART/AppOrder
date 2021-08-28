namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_DTLCOM")]
    public partial class M_SUBLEG_DTLCOM
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
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        public double? CRAMT { get; set; }

        public short? CRDAYS { get; set; }

        [StringLength(6)]
        public string AREACD { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(4)]
        public string DISCRTCD { get; set; }

        public virtual M_SUBLEG M_SUBLEG { get; set; }
    }
}
