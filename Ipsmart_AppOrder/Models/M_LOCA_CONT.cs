namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_LOCA_CONT")]
    public partial class M_LOCA_CONT
    {
        [Key]
        [StringLength(35)]
        public string CMPNYNM { get; set; }

        [Required]
        [StringLength(5)]
        public string LWRTDSLMT { get; set; }

        [Required]
        [StringLength(5)]
        public string LWRTDSRT { get; set; }
    }
}
