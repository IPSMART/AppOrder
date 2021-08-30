using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Models
{
    public class ImprovarDB : DbContext, IDbModelCacheKeyProvider
    {
        public string SchemaDBO { get; private set; }
        internal ImprovarDB(string Constring, string schemaname) : base(new OracleConnection(Constring), true)
        {
            SchemaDBO = schemaname;
        }
        public ImprovarDB() : base("name=local1")
        {
            Database.SetInitializer<ImprovarDB>(null);
        }
        
        public virtual DbSet<T_RETAILORDERDTL> T_RETAILORDERDTL { get; set; }
        public virtual DbSet<T_RETAILORDER> T_RETAILORDER { get; set; }
        public virtual DbSet<T_TXNEWB> T_TXNEWB { get; set; }
        public virtual DbSet<T_TXNEINV> T_TXNEINV { get; set; }
        public virtual DbSet<MS_MUSRACS> MS_MUSRACS { get; set; }
        public virtual DbSet<M_SUBLEG_GRP> M_SUBLEG_GRP { get; set; }
        public virtual DbSet<M_SUBLEG_GRPCLASS1> M_SUBLEG_GRPCLASS1 { get; set; }
        public virtual DbSet<M_SUBLEG_GRPHDR> M_SUBLEG_GRPHDR { get; set; }
        public virtual DbSet<M_REPFORMAT> M_REPFORMAT { get; set; }
        public virtual DbSet<T_TXNSTATUS> T_TXNSTATUS { get; set; }
        public virtual DbSet<M_SYSCNFG> M_SYSCNFG { get; set; }
        public virtual DbSet<M_BRGRP> M_BRGRP { get; set; } 
        public virtual DbSet<M_SDR_QUERY> M_SDR_QUERY { get; set; }
        public virtual DbSet<M_LOCKDATA> M_LOCKDATA { get; set; }
        public virtual DbSet<SD_COMPANY> SD_COMPANY { get; set; }
        public virtual DbSet<USER_GUIDE> USER_GUIDE { get; set; }
        public virtual DbSet<M_SUBLEG_LOCOTH> M_SUBLEG_LOCOTH { get; set; }
        public virtual DbSet<M_SUBLEG_SDDTL> M_SUBLEG_SDDTL { get; set; }
        public virtual DbSet<M_SUBLEG_SDDTL_ITGRP> M_SUBLEG_SDDTL_ITGRP { get; set; }
        public virtual DbSet<M_SIGN_AUTH> M_SIGN_AUTH { get; set; }
        public virtual DbSet<M_CLASS1> M_CLASS1 { get; set; }
        public virtual DbSet<M_CNTRL_AUTH> M_CNTRL_AUTH { get; set; }
        public virtual DbSet<M_CNTRL_HDR> M_CNTRL_HDR { get; set; }
        public virtual DbSet<M_CNTRL_HDR_DOC> M_CNTRL_HDR_DOC { get; set; }
        public virtual DbSet<M_CNTRL_HDR_DOC_DTL> M_CNTRL_HDR_DOC_DTL { get; set; }
        public virtual DbSet<M_CNTRL_LOCA> M_CNTRL_LOCA { get; set; }
        public virtual DbSet<M_DOCTYPE> M_DOCTYPE { get; set; }
        public virtual DbSet<M_DTYPE> M_DTYPE { get; set; }
        public virtual DbSet<M_GROUPDOCCD> M_GROUPDOCCD { get; set; }
        public virtual DbSet<M_SITEM> M_SITEM { get; set; }
        public virtual DbSet<M_SITEMBOM> M_SITEMBOM { get; set; }
        public virtual DbSet<M_SITEMBOMDTL> M_SITEMBOMDTL { get; set; }
        public virtual DbSet<M_SITEMBOMSFDTL> M_SITEMBOMSFDTL { get; set; }
        public virtual DbSet<M_USR_ACS> M_USR_ACS { get; set; }
        public virtual DbSet<M_USR_ACS_DOCCD> M_USR_ACS_DOCCD { get; set; }
        public virtual DbSet<M_USR_ACS_GRPDTL> M_USR_ACS_GRPDTL { get; set; }
        public virtual DbSet<T_CNTRL_AUTH> T_CNTRL_AUTH { get; set; }
        public virtual DbSet<T_CNTRL_HDR> T_CNTRL_HDR { get; set; }
        public virtual DbSet<T_CNTRL_HDR_DOC> T_CNTRL_HDR_DOC { get; set; }
        public virtual DbSet<T_CNTRL_HDR_DOC_DTL> T_CNTRL_HDR_DOC_DTL { get; set; }
        public virtual DbSet<T_CNTRL_HDR_REM> T_CNTRL_HDR_REM { get; set; }
        public virtual DbSet<USER_APPL> USER_APPL { get; set; }
        public virtual DbSet<USER_ACTIVITY> USER_ACTIVITY { get; set; }
        public virtual DbSet<MS_DOCCTG> MS_DOCCTG { get; set; }
        public virtual DbSet<M_GENLEG> M_GENLEG { get; set; }
        public virtual DbSet<M_COMP> M_COMP { get; set; }
        public virtual DbSet<M_LOCA> M_LOCA { get; set; }
        public virtual DbSet<M_CNTRL_HDR_REM> M_CNTRL_HDR_REM { get; set; }
        public virtual DbSet<M_SUBLEG_BUSNAT> M_SUBLEG_BUSNAT { get; set; }
        public virtual DbSet<MS_GSTUOM> MS_GSTUOM { get; set; }
        public virtual DbSet<MS_STATE> MS_STATE { get; set; }
        public virtual DbSet<MS_COUNTRY> MS_COUNTRY { get; set; }
        public virtual DbSet<M_DISTRICT> M_DISTRICT { get; set; }
        public virtual DbSet<M_PARTYGRP> M_PARTYGRP { get; set; }
        public virtual DbSet<M_SUBLEG> M_SUBLEG { get; set; }
        public virtual DbSet<M_SUBLEG_GL> M_SUBLEG_GL { get; set; }
        public virtual DbSet<M_SUBLEG_TAX> M_SUBLEG_TAX { get; set; }
        public virtual DbSet<M_SUBLEG_CONT> M_SUBLEG_CONT { get; set; }
        public virtual DbSet<M_DESIGNATION> M_DESIGNATION { get; set; }
        public virtual DbSet<M_LOCA_IFSC> M_LOCA_IFSC { get; set; }
        public virtual DbSet<M_LOCA_CONT> M_LOCA_CONT { get; set; }
        public virtual DbSet<M_SUBLEG_IFSC> M_SUBLEG_IFSC { get; set; }
        public virtual DbSet<M_SUBLEG_LINK> M_SUBLEG_LINK { get; set; }
        public virtual DbSet<MS_NATBUSCODES> MS_NATBUSCODES { get; set; }
        public virtual DbSet<M_UOM> M_UOM { get; set; }
        public virtual DbSet<T_TXNTRANS> T_TXNTRANS { get; set; }
        public virtual DbSet<T_CNTRL_DOC_PASS> T_CNTRL_DOC_PASS { get; set; }
        public virtual DbSet<M_DOC_AUTH> M_DOC_AUTH { get; set; }
        public virtual DbSet<M_MONTH> M_MONTH { get; set; }
        public virtual DbSet<MS_LINK> MS_LINK { get; set; }
        public virtual DbSet<MS_COMPTYPE> MS_COMPTYPE { get; set; }

        public virtual DbSet<MS_BANKIFSC> MS_BANKIFSC { get; set; }
        public virtual DbSet<M_RETAILOUTLET> M_RETAILOUTLET { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (SchemaDBO != null)
            {
                Database.SetInitializer<ImprovarDB>(null);
                modelBuilder.HasDefaultSchema(SchemaDBO);
            }

            base.OnModelCreating(modelBuilder);
        }
        public string CacheKey
        {
            get { return SchemaDBO; }
        }

    }
}