namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_SDDTL_ITGRP")]
    public partial class M_SUBLEG_SDDTL_ITGRP
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

        [Key]
        [Column(Order = 3)]
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(4)]
        public string DISCRTCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CRLIMIT { get; set; }

        public short? CRDAYS { get; set; }
        [StringLength(20)]
        public string SAPCD { get; set; }

        public long M_AUTONO { get; set; }
        [StringLength(8)]
        public string AGSLCD { get; set; }
        [StringLength(8)]
        public string SLMSLCD { get; set; }
    }
}
