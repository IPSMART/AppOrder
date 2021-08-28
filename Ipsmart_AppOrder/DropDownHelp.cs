using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Ajax.Utilities;

namespace Improvar
{
    public class DropDownHelp
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public List<DropDown_list_GLCD> DropDown_list_GLCD(string linkcd = "", string slcdmust = "N,Y")
        {
            List<DropDown_list_GLCD> dropDownlistGLCD = new List<DropDown_list_GLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string gllinkcd = linkcd;
                string glslcdmust = "Y,N";
                if (slcdmust != "") glslcdmust = slcdmust;
                if (linkcd == "") linkcd = "B,S,D,C,O";

                string[] lstlinkcd = linkcd.Split(',');
                string[] lstslcdmst = glslcdmust.Split(',');

                dropDownlistGLCD = (from j in DBF.M_GENLEG
                                    where lstlinkcd.Contains(j.LINKCD) && lstslcdmst.Contains(j.SLCDMUST)
                                    select new DropDown_list_GLCD
                                    {
                                        text = j.GLNM,
                                        value = j.GLCD
                                    }).ToList().OrderBy(s => s.text).ToList();
            }
            return dropDownlistGLCD;
        }
        public List<DropDown_list_SLCD> DropDown_list_SLCD()
        {
            List<DropDown_list_SLCD> dropDownlistsLCD = new List<DropDown_list_SLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                dropDownlistsLCD = (from j in DBF.M_SUBLEG select new DropDown_list_SLCD() { text = j.SLNM, value = j.SLCD,
                    //City = j.DISTRICT
                }).OrderBy(s => s.text).ToList();
            }
            return dropDownlistsLCD;
        }
        public List<DropDown_list_Class1> DropDown_list_Class1()
        {
            List<DropDown_list_Class1> dropDownlistsLClass1 = new List<DropDown_list_Class1>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                dropDownlistsLClass1 = (from j in DBF.M_CLASS1 select new DropDown_list_Class1() { text = j.CLASS1NM, value = j.CLASS1CD }).OrderBy(s => s.text).ToList();
            }
            return dropDownlistsLClass1;
        }
     
        public List<DropDown_list_SLCD> GetSlcdforSelection(string linkcd)
        {
            List<DropDown_list_SLCD> sllist = new List<DropDown_list_SLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                if (linkcd == "ALL" || linkcd == "")
                {
                    linkcd = string.Join(",", (from a in DBF.M_SUBLEG_LINK select a.LINKCD).Distinct().ToList());
                }
                string[] linkcode = linkcd.Split(',');
                sllist = (from a in DBF.M_SUBLEG
                          join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                          from b in x.DefaultIfEmpty()
                          join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                          join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                          from c in g.DefaultIfEmpty()
                          join d in DBF.M_PARTYGRP on a.PARTYCD equals d.PARTYCD into h
                          from d in h.DefaultIfEmpty()
                          where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                          select new DropDown_list_SLCD()
                          {
                              text = a.SLNM,
                              value = a.SLCD,
                              //City = a.SLAREA == null ? a.DISTRICT : a.SLAREA,
                              //PartyGrp = d.PARTYNM
                          }).DistinctBy(B => B.value).OrderBy(A => A.text).ToList();
            }
            return sllist;
        }
        public List<DropDown_list_AGSLCD> GetAgSlcdforSelection()
        {
            List<DropDown_list_AGSLCD> sllist = new List<DropDown_list_AGSLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                string[] linkcode = { "A" };
                sllist = (from a in DBF.M_SUBLEG
                          join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                          from b in x.DefaultIfEmpty()
                          join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                          join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                          from c in g.DefaultIfEmpty()
                          where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                          select new DropDown_list_AGSLCD()
                          {
                              text = a.SLNM,
                              value = a.SLCD
                          }).OrderBy(A => A.text).ToList();
            }
            return sllist;
        }
        public List<DropDown_list_SLMSLCD> GetSlmSlcdforSelection()
        {
            List<DropDown_list_SLMSLCD> sllist = new List<DropDown_list_SLMSLCD>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                string[] linkcode = { "M" };
                sllist = (from a in DBF.M_SUBLEG
                          join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                          from b in x.DefaultIfEmpty()
                          join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                          join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                          from c in g.DefaultIfEmpty()
                          where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                          select new DropDown_list_SLMSLCD()
                          {
                              text = a.SLNM,
                              value = a.SLCD
                          }).OrderBy(A => A.text).ToList();
            }
            return sllist;
        }
        public List<DropDown_list_Class1> GetClass1cdforSelection(string linkcd)
        {
            List<DropDown_list_Class1> sllist = new List<DropDown_list_Class1>();
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                sllist = (from a in DBF.M_CLASS1
                          join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                          where (i.INACTIVE_TAG != "Y")
                          select new DropDown_list_Class1()
                          {
                              text = a.CLASS1NM,
                              value = a.CLASS1CD
                          }).DistinctBy(B => B.value).OrderBy(A => A.text).ToList();
            }
            return sllist;
        }
 
        public List<DropDown_list_SubLegGrp> GetSubLegGrpforSelection(string grpcd="")
        {
            List<DropDown_list_SubLegGrp> sllist = new List<DropDown_list_SubLegGrp>();
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);

            sql += "select a.grpcd, b.grpnm, a.slcdgrpcd, a.slcdgrpnm, b.class1cd from ";
            sql += "(select a.grpcd, a.slcdgrpcd, decode(nvl(b.slcdgrpnm,''),'','',b.slcdgrpnm||' - ')||a.slcdgrpnm slcdgrpnm, a.grpcdfull ";
            sql += "from "+  scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grp b ";
            //sql += "where a.parentcd=b.slcdgrpcd(+) and a.slcd is null and a.parentcd is not null ) a, ";
            sql += "where a.parentcd=b.slcdgrpcd(+) and a.slcd is null ) a, ";

            sql += "(select a.grpcd, a.grpnm, b.class1cd ";
            sql += "from " + scmf + ".m_subleg_grphdr a, "+ scmf + ".m_subleg_grpclass1 b ";
            sql += "where a.grpcd=b.grpcd(+) ) b ";
            sql += "where a.grpcd=b.grpcd(+) ";

            sql = "select distinct grpcd, grpnm, slcdgrpcd, slcdgrpnm from ( " + sql + " ) order by grpnm, slcdgrpnm ";
            DataTable tbl = masterHelp.SQLquery(sql);

            sllist = (from DataRow dr in tbl.Rows
                      select new DropDown_list_SubLegGrp()
                      {
                          text = dr["slcdgrpnm"].ToString(),
                          value = dr["slcdgrpcd"].ToString(),
                          grpnm = dr["grpnm"].ToString()
                      }).ToList();
            return sllist;
        }

    }
}