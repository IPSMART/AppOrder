namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_SLM_DLYREP")]
    public partial class T_SLM_DLYREP
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
        [Column(Order = 1)]
        public byte SLNO { get; set; }

        public DateTime? DTD { get; set; }

        [StringLength(30)]
        public string PLFROM { get; set; }

        [StringLength(30)]
        public string PLTO { get; set; }

        [StringLength(10)]
        public string MODETRAVEL { get; set; }

        public short? KMUPDN { get; set; }

        [StringLength(40)]
        public string CONVSTR { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CONVAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TAAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DAAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? BOOKQTY { get; set; }

        [StringLength(4)]
        public string BOOKUOM { get; set; }

        [StringLength(200)]
        public string REMK { get; set; }

    }
}
