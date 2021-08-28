namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_BRGRP")]
    public partial class M_BRGRP
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
        [StringLength(4)]
        public string BRGRPCD { get; set; }

        [Required]
        [StringLength(30)]
        public string BRGRPNM { get; set; }

        [StringLength(40)]
        public string BLTOPHEAD { get; set; }

        [StringLength(1000)]
        public string FULLDESC { get; set; }

        [StringLength(25)]
        public string GRPNM { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(1)]
        public string ITEMTYPE { get; set; }

        [StringLength(8)]
        public string SALGLCD { get; set; }

        [StringLength(8)]
        public string PURGLCD { get; set; }
        [StringLength(8)]
        public string SALRETGLCD { get; set; }

        [StringLength(8)]
        public string PURRETGLCD { get; set; }
        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(8)]
        public string CLASS2CD { get; set; }
        [StringLength(1)]
        public string BATCHWISESTK { get; set; }
        [Required]
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        [StringLength(4)]
        public string PRODGRPCD { get; set; }
        [StringLength(8)]
        public string HSNSACCD { get; set; }
        public string NEGSTK { get; set; }
        public string RATEQNTYBAG { get; set; }
        public string STKQNTYBAG { get; set; }
        public string RATECHANGE { get; set; }
        public string QNTYAUTOCAL { get; set; }
        public virtual M_CNTRL_HDR M_CNTRL_HDR { get; set; }
    }
}
