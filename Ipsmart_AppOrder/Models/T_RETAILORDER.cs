namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_RETAILORDER")]
    public partial class T_RETAILORDER
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(30)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(8)]
        public string RTLCD { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        public double? DOCAMT { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(40)]
        public string LM_USR_ID { get; set; }

        public DateTime? LM_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string LM_USR_SIP { get; set; }

        [StringLength(40)]
        public string DEL_USR_ID { get; set; }

        public DateTime? DEL_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string DEL_USR_SIP { get; set; }

        [StringLength(1)]
        public string CANCEL { get; set; }

        [StringLength(100)]
        public string CANC_REM { get; set; }

        [StringLength(40)]
        public string CANC_USR_ID { get; set; }

        public DateTime? CANC_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string CANC_USR_SIP { get; set; }

        [StringLength(100)]
        public string LM_REM { get; set; }

        [StringLength(100)]
        public string DEL_REM { get; set; }

        public double? GPSLAT { get; set; }

        public double? GPSLOT { get; set; }

        [StringLength(500)]
        public string GPSNM { get; set; }

        [StringLength(200)]
        public string DOCREM { get; set; }
        public int VCHRNO { get; set; }
        [StringLength(4)]
        public string MNTHCD { get; set; }

    }
}
