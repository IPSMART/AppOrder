namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_CNTRL_LOCA")]
    public partial class M_CNTRL_LOCA
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long M_AUTONO { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Key]
        [StringLength(4)]
        [Column(Order = 1)]
        public string COMPCD { get; set; }
        
    }
}
