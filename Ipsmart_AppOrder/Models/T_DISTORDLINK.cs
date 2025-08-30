namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_DISTORDLINK")]
    public partial class T_DISTORDLINK
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
        [Column(Order = 2)]
        [StringLength(30)]
        public string RTLAUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        public short SLNO { get; set; }

        [StringLength(50)]
        public string ORDSKIPREASON { get; set; }

    }
}
