namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_RETAILOUTLET")]
    public partial class M_RETAILOUTLET
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
        [StringLength(8)]
        public string RETLRCD { get; set; }

        [Required]
        [StringLength(100)]
        public string RETLRNM { get; set; }

        [StringLength(60)]
        public string RETLRADD1 { get; set; }

        [StringLength(60)]
        public string RETLRADD2 { get; set; }

        public long M_AUTONO { get; set; }

        public int? RETLRPIN { get; set; }

        [StringLength(60)]
        public string RETLRCITY { get; set; }

        [StringLength(60)]
        public string RETLRLOCALITY { get; set; }

        [StringLength(15)]
        public string RETLRGSTNO { get; set; }

        [StringLength(300)]
        public string REMARKS { get; set; }

        [StringLength(8)]
        public string DSTBRSLCD { get; set; }
    }
}
