namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEMBOM")]
    public partial class M_SITEMBOM
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
        public string BOMCD { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? BOMDT { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [Required]
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(20)]
        public string BOMREFNO { get; set; }

        [StringLength(1)]
        public string QNTYBAG { get; set; }

        [StringLength(50)]
        public string REMARKS { get; set; }

        public long M_AUTONO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]

        [Required]
        public double? BOMQNTY { get; set; }
        
    }
}
