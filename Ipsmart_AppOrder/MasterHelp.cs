using System;
using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Ajax.Utilities;
using Microsoft.Win32;
using System.Web;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Reflection;

namespace Improvar
{
    public class MasterHelp : MasterHelpFa
    {
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Connection Cn = new Connection();
         public string DOCCD_help(string val, string doctype = "", string doccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

            var query = (from c in DB.M_DOCTYPE orderby c.DOCNM select new { DOCTYPE = c.DOCTYPE, DOCNM = c.DOCNM, DOCCD = c.DOCCD, }).ToList();

            if (doctype != "") { string[] DOC_TYPE = doctype.Split(','); query = query.Where(a => a != null && query.Count > 0 && DOC_TYPE.Contains(a.DOCTYPE)).ToList(); }

            if (doccd != "") { string[] DOC_CODE = doccd.Split(','); query = query.Where(a => a != null && query.Count > 0 && DOC_CODE.Contains(a.DOCCD)).ToList(); }

            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].DOCNM + "</td><td>" + query[i].DOCCD + "</td></tr>");
                }
                var hdr = "Document Name" + Cn.GCS() + "Document Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.DOCCD == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = ToReturnFieldValues(query);
                    }
                    return str;
                }
                else
                {
                    return "Invalid Document Code ! Please Select / Enter a Valid Document Code !!";
                }
            }
        }
        public string AUTHCD_help(ImprovarDB DB)
        {
            var query = (from c in DB.M_SIGN_AUTH
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N"
                         select new
                         {
                             Code = c.AUTHCD,
                             Description = c.AUTHNM
                         }).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            for (int i = 0; i <= query.Count - 1; i++)
            {
                SB.Append("<tr><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
            }
            var hdr = "Authority Name" + Cn.GCS() + "Authority Code";
            return Generate_help(hdr, SB.ToString());
        }
        public string DOCNO_help(string Code, string LOCCD, string COMPCD, string val, string doctype = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "select a.doconlyno,a.docdt,a.doccd,b.glnm,c.slnm from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr a," + CommVar.CurSchema(UNQSNO) + ".m_genleg b,";
            sql += " " + CommVar.CurSchema(UNQSNO) + ".m_subleg c where ";
            if (val != null) sql += "a.doconlyno = '" + val + "'and";
            sql += "  a.doccd='" + Code + "' and a.loccd='" + LOCCD + "'and a.compcd='" + COMPCD + "'and a.yr_cd='" + CommVar.YearCode(UNQSNO) + "' and a.glcd=b.glcd(+) and a.slcd = c.slcd(+) ";
            sql += " order by a.doconlyno ";
            var query = SQLquery(sql);

            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query.Rows[i]["doccd"] + "</td><td>" + query.Rows[i]["doconlyno"] + "</td><td>" + query.Rows[i]["docdt"].retDateStr() + "</td><td>" + query.Rows[i]["glnm"] + "</td><td>" + query.Rows[i]["slnm"] + "</td></tr>");
                }
                var hdr = "Document Code" + Cn.GCS() + "Document No." + Cn.GCS() + "Document Date" + Cn.GCS() + "General ledger name " + Cn.GCS() + "Sub ledger name";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                if (query.Rows.Count != 0)
                {
                    string str = "";
                    str = ToReturnFieldValues("", query);
                    return str;
                }
                else
                {
                    return "0";
                }
            }
        }
        public void ExcelfromDataTables(DataTable[] dt, string[] sheetname, string filename, bool isRowHighlight, string Caption)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    for (int i = 0; i < dt.Length; i++)
                    {
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheetname[i]);
                        int row = 0;
                        if (Caption != "")
                        {
                            ws.Cells[++row, 1].Value = CommVar.CompName(UNQSNO);
                            ws.Cells[++row, 1].Value = CommVar.LocName(UNQSNO);
                            ws.Cells[++row, 1].Value = Caption;
                        }
                        if (isRowHighlight)
                        {
                            ws.Cells[++row, 1].LoadFromDataTable(dt[i], true, TableStyles.Medium15); //You can Use TableStyles property of your desire.    ,
                        }
                        else
                        {
                            using (ExcelRange Rng = ws.Cells[row + 1, 1, row + 1, dt[i].Columns.Count])
                            {
                                Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                Rng.Style.Font.Bold = true;
                                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                            }
                            ws.Cells[++row, 1].LoadFromDataTable(dt[i], true);
                        }
                        int strtRow = row + 1;
                        row = row + dt[i].Rows.Count;
                        using (ExcelRange Rng = ws.Cells[++row, 1, row, dt[i].Columns.Count])
                        {
                            Rng.Style.Font.Bold = true;
                        }
                        ws.Cells[row, 1].Value = "Sub Total";
                        int column = 0;
                        foreach (DataColumn dc in dt[i].Columns)
                        {
                            ++column;
                            if (dc.DataType == typeof(double) || dc.DataType == typeof(decimal) || dc.DataType == typeof(int))
                            {
                                ws.Cells[row, column, row, column].Formula = "=sum(" + ws.Cells[strtRow, column].Address + ":" + ws.Cells[row - 1, column].Address + ")";
                            }
                        }
                    }
                    //Read the Excel file in a byte array    
                    Byte[] fileBytes = pck.GetAsByteArray();
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + filename.retRepname() + ".xlsx");
                    HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    HttpContext.Current.Response.BinaryWrite(fileBytes);
                    HttpContext.Current.Response.End();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }

        public string ASSETNO_HELP(string val, string astno, string FIXACD)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    string sql = ""; string schm = CommVar.CurSchema(UNQSNO);
                    sql += "    select distinct a.autoslno, a.autono, a.slno, a.astno, c.astdesc, c.dt, c.suprefno, c.suprefdt,c.slcd ,";
                    sql += "      a.qty orgqty, nvl(b.qty, 0) salqty, a.qty - nvl(b.qty, 0)balqty from ";
                    sql += "          (select a.autono || a.slno autoslno, a.autono, a.slno, a.astno, a.qty ";
                    sql += "          from " + schm + ".t_fixastdtl a, " + schm + ".t_fixast b ";
                    sql += "          where a.autono = b.autono ) a,   ";

                    sql += "      (select a.astautono || a.astslno astautoslno, a.astautono, a.astslno, sum(a.qty) qty ";
                    sql += "       from " + schm + ".t_fixastsaledtl a, " + schm + ".t_cntrl_hdr c ";
                    sql += "      where a.autono = c.autono(+) and ";
                    sql += "      c.compcd = '" + CommVar.Compcd(UNQSNO) + "' and c.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(c.cancel, 'N')= 'N' ";
                    sql += "      group by a.astautono || a.astslno, a.astautono, a.astslno ) b, ";
                    sql += "      " + schm + ".t_fixast c, " + schm + ".t_cntrl_hdr d ";
                    sql += "      where a.autoslno = b.astautoslno(+) and a.autono = c.autono(+) and ";
                    if (FIXACD.retStr() != "") sql += " c.fixacd = '" + FIXACD + "' and d.compcd = '" + CommVar.Compcd(UNQSNO) + "' and d.loccd = '" + CommVar.Loccd(UNQSNO) + "' and ";
                    if (val.retStr() != "") sql += " a.autono = '" + val + "'and ";
                    if (astno.retStr() != "") sql += " a.astno = '" + astno + "' and";
                    sql += "   a.autono = d.autono(+) and a.qty - nvl(b.qty, 0) > 0 ";
                    var query = SQLquery(sql);
                    if (val == null)
                    {
                        System.Text.StringBuilder SB = new System.Text.StringBuilder();

                        for (int i = 0; i <= query.Rows.Count - 1; i++)
                        {
                            SB.Append("<tr><td>" + query.Rows[i]["autono"] + "</td><td>" + query.Rows[i]["astdesc"] + "</td><td>" + query.Rows[i]["astno"] + "</td><td>"
                                + query.Rows[i]["balqty"] + "</td></tr>");
                        }
                        var hdr = "Asset No." + Cn.GCS() + "Asset Name" + Cn.GCS() + "Asset No." + Cn.GCS() + "Stk.Qnty";
                        return Generate_help(hdr, SB.ToString());
                    }
                    else
                    {
                        if (query.Rows.Count != 0)
                        {
                            string str = "";
                            str = ToReturnFieldValues("", query);
                            return str;
                        }
                        else
                        {
                            return "0";
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                return "";
            }

        }
        public string ASSET_ATONO_HELP(string val, string FIXACD, string ASTAUTONO)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    string schm = CommVar.CurSchema(UNQSNO);
                    string schmf = CommVar.FinSchema(UNQSNO);
                    string sql = "";
                    sql += "select distinct  b.autono,b.astno,a.astdesc, a.dt, a.suprefno, a.suprefdt,a.slcd,d.slnm,a.floccd,e.flocnm from ";
                    sql += "" + schm + ".t_fixast a," + schm + ".t_fixastdtl b, " + schm + ".t_cntrl_hdr c, " + schmf + ".m_subleg d, " + schm + ".m_fixloca e ";
                    sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and ";
                    sql += "a.slcd = d.slcd(+) and a.floccd=e.floccd(+) ";
                    sql += "and c.compcd = '" + CommVar.Compcd(UNQSNO) + "' and c.loccd = '" + CommVar.Loccd(UNQSNO) + "' ";

                    if (FIXACD.retStr() != "") sql += "and a.fixacd = '" + FIXACD + "' ";
                    if (val.retStr() != "") sql += "and b.astno = '" + val + "' ";
                    if (ASTAUTONO.retStr() != "") sql += "and b.autono = '" + ASTAUTONO + "' ";
                    sql += "order by b.autono ";
                    var query = SQLquery(sql);
                    if (val == null)
                    {
                        System.Text.StringBuilder SB = new System.Text.StringBuilder();

                        for (int i = 0; i <= query.Rows.Count - 1; i++)
                        {
                            SB.Append("<tr><td>" + query.Rows[i]["astno"] + "</td><td>" + query.Rows[i]["astdesc"] + "</td><td>" + query.Rows[i]["autono"] + "</td></tr>");
                        }
                        var hdr = "Asset No." + Cn.GCS() + "Asset Name" + Cn.GCS() + "Asset AutoNo.";
                        return Generate_help(hdr, SB.ToString());
                    }
                    else
                    {
                        if (query.Rows.Count != 0)
                        {
                            string str = "";
                            str = ToReturnFieldValues("", query);
                            return str;
                        }
                        else
                        {
                            return "0";
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                return "";
            }
        }
        public string INV_ITCD_HELP(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    string str = "select c.ITCD, c.ITDESCN itnm, c.UOM,j.UOMNM,c.BATCHOP ";
                    str += " from " + CommVar.InvSchema(UNQSNO) + ".M_ITEM c, " + CommVar.InvSchema(UNQSNO) + ".M_CNTRL_HDR i, " + CommVar.FinSchema(UNQSNO) + ".M_UOM j ";
                    str += "where c.M_AUTONO = i.M_AUTONO and i.INACTIVE_TAG = 'N' and j.UOMCD = c.UOM(+)  ";
                    if (val != null) str += "and c.ITCD = '" + val + "' ";
                    DataTable query = SQLquery(str);
                    if (val == null)
                    {
                        System.Text.StringBuilder SB = new System.Text.StringBuilder();
                        for (int i = 0; i <= query.Rows.Count - 1; i++)
                        {
                            SB.Append("<tr><td>" + query.Rows[i]["itnm"].retStr() + "</td><td>" + query.Rows[i]["ITCD"].retStr() + " </td><td> " + query.Rows[i]["UOM"].retStr() + "</td><td>" + query.Rows[i]["UOMNM"].retStr() + " </td></tr>");
                        }
                        var hdr = "Item Name" + Cn.GCS() + "Item Code" + Cn.GCS() + "UOM Code" + Cn.GCS() + "UOM Name";
                        return Generate_help(hdr, SB.ToString());
                    }
                    else
                    {
                        if (query.Rows.Count != 0)
                        {
                            str = "";
                            str = ToReturnFieldValues("", query);
                            return str;
                        }
                        else
                        {
                            return "0";
                        }
                    }
                }
            }
            catch (Exception EX)
            {
                return "";
            }
        }
        public DataTable Get_Fixast_Component_Col(string GRPCD, bool ONLY_DATECOL = false)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    string sql = "";
                    sql += "select i.FLDCD,nvl(i.FLDDESC,i.FLDNM)FLDNM,i.FLDTYPE,i.FLDLEN,i.FLDDEC,i.FLDMANDT,i.FLDREMIND,i.FLDDATACOMBO,i.FLDFLAG1,i.DEACTIVATE ";
                    sql += "from " + CommVar.CurSchema(UNQSNO) + ".M_FIXGRP_COMP i ";
                    sql += "where i.GRPCD = '" + GRPCD + "' ";
                    if (ONLY_DATECOL == true)
                    {
                        sql += "and i.FLDTYPE = 'D' and  nvl(i.FLDREMIND, 'N') = 'Y' ";
                    }
                    DataTable tbl_comp = SQLquery(sql);

                    DataTable tbl = new DataTable();
                    tbl.Columns.Add("colval", typeof(string));
                    tbl.Columns.Add("coldesc", typeof(string));

                    int rNo = 0;
                    tbl.Rows.Add("");
                    tbl.Rows[rNo]["colval"] = "Asset No.";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^ASTNO" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^30" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Asset desc.";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^ASTDESC" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^70" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Bought Code";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^SLCD" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^8" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Bought from";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^SLNM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^45" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Department Name";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^DEPTNM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^20" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Supplier Ref No";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^SUPREFNO" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^20" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Supper Ref Date";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^SUPREFDT" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^D" + Cn.GCS()
                        + "^COLLEN=^10" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Location";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^FLOCCD" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^6" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Location Name";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^FLOCNM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^30" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Date of Purchase";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^DT" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^D" + Cn.GCS()
                        + "^COLLEN=^10" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Held by Code";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^EMPCD" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^8" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Held by Name";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^EMPNM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^45" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Held by Mobile No";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^EMPMOBILE" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^10" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Original Cost";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^ORGCOST" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^15" + Cn.GCS() + "^COLDEC=^2" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Other Exps";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^OTHEXPS" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^15" + Cn.GCS() + "^COLDEC=^2" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Bill Amt";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^BLAMT" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^15" + Cn.GCS() + "^COLDEC=^2" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Extra Desc.";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^EXTRADTL" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^70" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Driver Code";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^DRIVERCD" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^8" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Driver Name";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^DRIVERNM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^45" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Driver Mobile No.";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^DRIVERMOBILE" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^10" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Last Activity On";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^LASTACTVON" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^D" + Cn.GCS()
                        + "^COLLEN=^10" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Quantity";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^QTY" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^6" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Sales Quantity";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^SALQTY" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^6" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Balance Quantity";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^BALQTY" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^N" + Cn.GCS()
                        + "^COLLEN=^6" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Remarks";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^TREM" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^250" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();

                    tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                    tbl.Rows[rNo]["colval"] = "Flag";
                    tbl.Rows[rNo]["coldesc"] = "^COLNM=^FLAG1" + Cn.GCS() + "^COLMODE=^S" + Cn.GCS() + "^COLTYPE=^C" + Cn.GCS()
                        + "^COLLEN=^50" + Cn.GCS() + "^COLDEC=^" + Cn.GCS();



                    for (int i = 0; i <= tbl_comp.Rows.Count - 1; i++)
                    {
                        tbl.Rows.Add(""); rNo = tbl.Rows.Count - 1;
                        tbl.Rows[rNo]["colval"] = tbl_comp.Rows[i]["FLDNM"].retStr();
                        tbl.Rows[rNo]["coldesc"] = "^COLNM=^" + tbl_comp.Rows[i]["FLDCD"].retStr() + Cn.GCS() + "^COLMODE=^C" + Cn.GCS() + "^COLTYPE=^" + tbl_comp.Rows[i]["FLDTYPE"].retStr() + Cn.GCS()
                            + "^COLLEN=^" + tbl_comp.Rows[i]["FLDLEN"].retStr() + Cn.GCS() + "^COLDEC=^" + tbl_comp.Rows[i]["FLDDEC"].retStr() + Cn.GCS();
                    }


                    return tbl;
                }
            }
            catch (Exception EX)
            {
                return null;
            }
        }
        public DataTable Get_StaticField_Data(string GRPCD, string FIXACD, string FDATE, string TDATE, string ASTAUTONO, bool STATUS = false, bool onlylocadata = false)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scm = CommVar.CurSchema(UNQSNO);
                string compcd = CommVar.Compcd(UNQSNO);
                string loccd = CommVar.Loccd(UNQSNO);

                string sql = "";
                sql += " select a.autoslno, a.autono, a.slno, a.astno,a.extradtl, a.astdesc, a.dt, ";
                sql += "a.suprefno, a.suprefdt, a.slcd, a.slnm, ";
                sql += "a.orgcost,a.othexps, a.blamt, ";
                sql += "a.deptnm, a.floccd, a.empcd, a.drivercd, a.trem, a.flag1, a.lastactvon, ";
                sql += "a.flocnm, a.empnm, a.empmobile, a.drivernm, a.drivermobile, ";
                sql += "a.qty, a.salqty, a.balqty from( ";

                sql += " select a.autoslno, a.autono, a.slno, a.astno,a.extradtl, c.astdesc, c.dt, ";
                sql += "c.suprefno, c.suprefdt, c.slcd, e.slnm, ";
                sql += "c.orgcost,c.othexps, c.supblamt blamt, ";
                sql += "x.deptnm, x.floccd, x.slcd empcd, x.drivercd, x.trem, x.flag1, x.docdt lastactvon, ";
                sql += "l.flocnm, m.slnm empnm, m.regmobile empmobile, n.slnm drivernm, n.regmobile drivermobile, ";
                sql += "a.qty, nvl(b.qty, 0) salqty, a.qty - nvl(b.qty, 0) balqty from ";
                sql += "(select a.autono || a.slno autoslno, a.autono, a.slno, a.astno,a.extradtl, a.qty ";
                sql += "from " + scm + ".t_fixastdtl a, " + scm + ".t_fixast b ";
                sql += "where a.autono = b.autono ) a, ";

                sql += "(select a.astautono || a.astslno astautoslno, a.astautono, a.astslno, sum(a.qty) qty ";
                sql += "  from " + scm + ".t_fixastsaledtl a, " + scm + ".t_cntrl_hdr c ";
                sql += "where a.autono = c.autono(+) and ";
                sql += "c.compcd = '" + compcd + "' and c.loccd = '" + loccd + "' and nvl(c.cancel, 'N')= 'N' ";
                sql += "group by a.astautono || a.astslno, a.astautono, a.astslno ) b,  ";

                sql += "(select astautono, docdt, slcd, drivercd, deptnm, floccd, trem, flag1 from ";
                sql += "(select a.astautono, c.docdt, a.slcd, a.drivercd, a.deptnm, a.floccd, a.trem, a.flag1,    ";
                sql += "row_number() over(partition by a.astautono order by c.docdt desc) as rn from ";
                sql += "" + scm + ".t_fixloca a, " + scm + ".t_cntrl_hdr c ";
                sql += "where a.autono = c.autono(+) and c.compcd = '" + compcd + "' and c.loccd = '" + loccd + "' and nvl(c.cancel, 'N')= 'N' ";
                if (FDATE.retStr() != "") sql += "and c.docdt >= to_Date('" + FDATE + "','dd/mm/yyyy') ";
                if (TDATE.retStr() != "") sql += "and c.docdt <= to_date('" + TDATE + "','dd/mm/yyyy') ";
                sql += ") ";
                if (STATUS == true) sql += "where rn = 1  ";
                sql += ") x, ";

                sql += "" + scm + ".t_fixast c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_subleg e, ";
                sql += "" + scm + ".m_fixloca l, " + scm + ".m_subleg m, " + scm + ".m_subleg n ";
                sql += "where a.autoslno = b.astautoslno(+) and a.autono = c.autono(+) and ";
                if (onlylocadata == true) sql += "a.autono = x.astautono and "; else sql += "a.autono = x.astautono(+) and ";
                sql += "c.slcd = e.slcd(+) and x.floccd = l.floccd(+) and x.slcd = m.slcd(+) and x.drivercd = n.slcd(+) and ";
                sql += "d.compcd = '" + compcd + "' and d.loccd = '" + loccd + "' and ";
                sql += "a.autono = d.autono(+) and a.qty - nvl(b.qty, 0) > 0 ";

                //if (FDATE.retStr() != "") sql += "and d.docdt >= to_Date('" + FDATE + "','dd/mm/yyyy') ";
                if (TDATE.retStr() != "") sql += "and d.docdt <= to_date('" + TDATE + "','dd/mm/yyyy') ";
                if (GRPCD.retStr() != "") sql += "and c.grpcd ='" + GRPCD + "' ";
                if (FIXACD.retStr() != "") sql += "and c.fixacd in(" + FIXACD + ") ";
                if (ASTAUTONO.retStr() != "") sql += "and c.autono in(" + ASTAUTONO + ") ";

                sql += ")a order by a.astdesc,a.astno ";
                //if (LASTDATA.retStr() != "")
                //{
                //    sql += "order by d.docdt desc,d.docno desc  ";
                //    string sql1 = "select * from (" + sql + ") ";
                //    sql = sql1;
                //}
                DataTable tbl = SQLquery(sql);
                return tbl;

            }
            catch (Exception EX)
            {
                return null;
            }
        }
        public string PARK_ENTRIES(string COM_CD, string LOC_CD, string menuID, string menuIndex, string uid, string path)
        {
            Connection cn = new Connection();
            var UNQSNO = cn.getQueryStringUNQSNO();
            var PreviousUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            var PermissionID1 = queryString.Get("MNUDET").ToString().Replace(" ", "+");
            string PermissionID = cn.Decrypt_URL(PermissionID1);
            string[] PermissionIDArray = PermissionID.Split('~');
            string currentPath = HttpContext.Current.Request.Path;
            currentPath = currentPath.Replace("Home", PermissionIDArray[2]);
            currentPath = currentPath.Replace("ReadParkRecord", "RetrivePark");
            string url = currentPath;
            string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO);
            string Userid = uid;
            INI Handel_ini = new INI();
            string[] keys = Handel_ini.GetEntryNames(uid, path);
            string[] entries = Array.FindAll(keys, element => element.StartsWith(ID, StringComparison.Ordinal));
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            SB.Append("<table id='helpmnu' class='table  table-striped table-bordered table-hover compact' cellpadding='3px' cellspacing='3px' width='100%'><thead style='background-color:#2965aa; color:white'><tr>");
            SB.Append("<th align='center' tabindex='0'>" + "Sl.No." + "</th>");
            SB.Append("<th  tabindex='1'>" + "Select Record(s) from park" + "</th>");
            SB.Append("<th  tabindex='2'>" + "Option" + "</th>");
            SB.Append("</tr></thead><tbody>");
            for (int i = 0; i <= entries.Length - 1; i++)
            {
                string[] spl = entries[i].Split('*');
                string datetime = spl[1].Replace('_', ' ');
                DateTime dt = DateTime.ParseExact(datetime, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                dt = dt.AddHours(72);
                if (dt < DateTime.Now)
                {
                    SB.Append("<tr><td>" + (i + 1).ToString() + "</td><td>" + spl[1] + "&nbsp;<span style='color:red'>(72 hours over)</span>" + "</td><td align='center' title='Delete Park' style='color:red' onclick =return&nbsp;deleteparkfromhelp('" + entries[i].ToString() + "','" + menuID + "','" + menuIndex + "');><strong>X</strong></td></tr>");
                }
                else
                {
                    SB.Append("<tr><td>" + (i + 1).ToString() + "</td><td onclick =return&nbsp;getparkfromhelp('" + entries[i].ToString() + "','" + url + "');>" + spl[1] + "</td><td align='center' title='Delete Park' style='color:red' onclick =return&nbsp;deleteparkfromhelp('" + entries[i].ToString() + "','" + menuID + "','" + menuIndex + "');><strong>X</strong></td></tr>");
                }
            }
            SB.Append("</tbody></table>");
            return SB.ToString();
        }

        public string COMPANY_HELP(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DB.M_COMP select new { COMPCD = c.COMPCD, COMPNM = c.COMPNM }).ToList();
            if (val == null)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + query[i].COMPNM + "</td><td>" + query[i].COMPCD + "</td></tr>");
                }
                var hdr = "Company Name" + Cn.GCS() + "Company Code";
                return Generate_help(hdr, SB.ToString());
            }
            else
            {
                query = query.Where(a => a.COMPCD == val).ToList();
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.COMPCD + Cn.GCS() + i.COMPNM;
                    }
                    return str;
                }
                else
                {
                    return "Invalid Company Code ! Please Select / Enter a Valid Company Code !!";
                }
            }
        }
        public DataTable Get_DynamicField_Data(string GRPCD, string FIXACD, string FDATE, string TDATE, string ASTAUTONO, bool STATUS = false, bool DATETRACKER = false)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {

                string scm = CommVar.CurSchema(UNQSNO);
                string compcd = CommVar.Compcd(UNQSNO);
                string loccd = CommVar.Loccd(UNQSNO);

                string sql1 = "";
                if (DATETRACKER == true)
                {
                    sql1 += "select a.autono, a.rn, a.fldcd, to_char(a.fldval,'dd/mm/yyyy') fldval, a.fldtype, ";
                    sql1 += "g.flddec, g.fldlen, g.fldremind, nvl(g.flddesc, g.fldnm) fldnm from ";
                    sql1 += "(select autono, fldcd, fldval, fldtype, docdt, rn from ";
                    sql1 += "(select a.autono, b.fldcd, to_date(b.fldval,'dd/mm/yyyy') fldval, b.fldtype, c.docdt, ";
                    sql1 += "row_number() over(partition by a.autono, b.fldcd order by c.docdt desc) as rn ";
                    sql1 += "from " + scm + ".t_fixast a, " + scm + ".t_fixastcomp b, " + scm + ".t_cntrl_hdr c ";
                    sql1 += "where a.autono = b.astautono(+) and b.autono = c.autono(+) and b.fldtype='D' ";
                    //if (DATETRACKER == false)
                    //{
                    //    if (FDATE.retStr() != "") sql1 += "and c.docdt >= to_Date('" + FDATE + "','dd/mm/yyyy') ";
                    //    if (TDATE.retStr() != "") sql1 += "and c.docdt <= to_date('" + TDATE + "','dd/mm/yyyy') ";
                    //}
                    if (GRPCD.retStr() != "") sql1 += "and a.grpcd ='" + GRPCD + "' ";
                    if (FIXACD.retStr() != "") sql1 += "and a.fixacd in(" + FIXACD + ") ";
                    if (ASTAUTONO.retStr() != "") sql1 += "and a.autono in(" + ASTAUTONO + ") ";
                    sql1 += " and c.compcd = '" + compcd + "' and c.loccd = '" + loccd + "' and nvl(c.cancel, 'N') = 'N' ) where rn=1) a, ";
                    sql1 += scm + ".m_fixgrp_comp g ";
                    sql1 += "where a.fldcd = g.fldcd(+) ";
                    if (STATUS == true) sql1 += "and a.rn=1 ";
                    sql1 += "and g.fldtype = 'D' and  nvl(g.fldremind, 'N') = 'Y' ";
                    if (FDATE.retStr() != "") sql1 += "and a.fldval >= to_Date('" + FDATE + "','dd/mm/yyyy') ";
                    if (TDATE.retStr() != "") sql1 += "and a.fldval <= to_date('" + TDATE + "','dd/mm/yyyy') ";
                    sql1 += "order by autono, fldcd, rn ";
                }
                else
                {
                    sql1 += "select a.autono, a.rn, a.fldcd, a.fldval, a.fldtype, ";
                    sql1 += "g.flddec, g.fldlen, g.fldremind, nvl(g.flddesc, g.fldnm) fldnm from ";
                    sql1 += "(select autono, fldcd, fldval, fldtype, docdt, rn from ";
                    sql1 += "(select a.autono, b.fldcd, b.fldval, b.fldtype, c.docdt, ";
                    sql1 += "row_number() over(partition by a.autono, b.fldcd order by c.docdt desc) as rn ";
                    sql1 += "from " + scm + ".t_fixast a, " + scm + ".t_fixastcomp b, " + scm + ".t_cntrl_hdr c ";
                    sql1 += "where a.autono = b.astautono(+) and b.autono = c.autono(+)  ";
                    //if (DATETRACKER == false)
                    //{
                    if (FDATE.retStr() != "") sql1 += "and c.docdt >= to_Date('" + FDATE + "','dd/mm/yyyy') ";
                    if (TDATE.retStr() != "") sql1 += "and c.docdt <= to_date('" + TDATE + "','dd/mm/yyyy') ";
                    //}
                    if (GRPCD.retStr() != "") sql1 += "and a.grpcd ='" + GRPCD + "' ";
                    if (FIXACD.retStr() != "") sql1 += "and a.fixacd in(" + FIXACD + ") ";
                    if (ASTAUTONO.retStr() != "") sql1 += "and a.autono in(" + ASTAUTONO + ") ";
                    sql1 += " and c.compcd = '" + compcd + "' and c.loccd = '" + loccd + "' and nvl(c.cancel, 'N') = 'N' ) ) a, ";
                    sql1 += scm + ".m_fixgrp_comp g ";
                    sql1 += "where a.fldcd = g.fldcd(+) ";
                    sql1 += "order by autono, fldcd, rn ";
                }
                DataTable tbl = SQLquery(sql1);


                return tbl;
            }
            catch (Exception EX)
            {
                return null;
            }
        }
        public double getSlcdTCSonCalc(string slcdpanno, string docdt, string menupara = "SB", string autono = "")
        {
            double rtval = 0;
            string sql = "", COM = CommVar.Compcd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string trcd = "'SB','SD'";
            string salpur = "S";
            if (menupara == "PB" || menupara == "PD") { trcd = "'PB','PD'"; salpur = "P"; }

            sql = "select sum(nvl(amt,0)) amt from (";

            sql += "select sum(a.amt) amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_cntrl_hdr c, " + scmf + ".m_subleg d, " + scmf + ".m_genleg e ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and a.slcd = d.slcd(+) and d.panno = '" + slcdpanno + "' and ";
            sql += "c.compcd = '" + COM + "' and b.trcd in (" + trcd + ") and nvl(c.cancel, 'N')= 'N' and ";
            if (autono.retStr() != "") sql += "a.autono not in ('" + autono + "') and ";
            sql += "a.glcd=e.glcd(+) and e.linkcd in (" + (salpur == "P" ? "'C'" : "'D'") + ") and ";
            sql += "c.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') ";

            sql += "union all ";

            sql += "select sum(case a.drcr when e.linkcd then a.amt else a.amt*-1 end) amt ";
            sql += "from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_hdr b, " + scmf + ".t_cntrl_hdr c, " + scmf + ".m_doctype d, ";
            sql += scmf + ".m_genleg e, " + scmf + ".m_subleg f ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.doccd=d.doccd(+) and ";
            sql += "c.compcd='" + COM + "' and nvl(c.cancel,'N')='N' and d.doctype in ('AOPEN') and a.drcr in ('D','C') and ";
            sql += "a.glcd=e.glcd(+) and a.slcd=f.slcd(+) and e.linkcd in (" + (salpur == "P" ? "'C'" : "'D'") + ") and f.panno='" + slcdpanno + "' ";

            sql += ") ";

            DataTable tbl = SQLquery(sql);
            if (tbl.Rows.Count == 1) rtval = tbl.Rows[0]["amt"].retDbl();
            return rtval;
        }
        public DataTable GenShareFile(string docdt, string invest = "", string ctgcd = "", string shrcd = "", string compcd = "", bool OnlyBal = false, string shrdpcd = "", string skipautono="")
        {
            try
            {
                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);

                string Str = ""; ;
                Str = "SELECT  H.COMPCD, A.AUTONO, H.DOCDT, B.SLCD, G.SLNM, H.DOCNO,E.COMPNM, B.SHRDPCD, F.SHRDPNM,NVL(B.TRADEDT, H.DOCDT)TRADEDT, h.compcd||h.loccd comploccd, e.compnm||' ['||j.locnm||']' complocnm, ";
                Str += "B.SLNO, B.SHRCD,B.QTY, B.COSTAMT, B.RATE, C.SHRNM,C.CTGCD, D.CTGNM, B.INVEST, C.SHRSECT ";
                Str += "FROM " + scmf + ".T_SHRTXN A, " + scmf + ".T_SHRTXNDTL B, " + scmf + ".M_SHRMST C, " + scmf + ".M_SHRCTG D, " + scmf + ".m_comp E, " + scmf + ".M_SHRDP F, ";
                Str += scmf + ".M_SUBLEG G, " + scmf + ".T_CNTRL_HDR H, " + scmf + ".M_DOCTYPE I, " + scmf + ".m_loca j ";
                Str += "WHERE A.AUTONO = B.AUTONO AND B.SHRCD = C.SHRCD(+)  AND C.CTGCD = D.CTGCD(+) AND H.COMPCD = E.COMPCD(+) AND B.SHRDPCD = F.SHRDPCD(+) AND B.SLCD = G.SLCD(+) ";
                Str += "and a.autono = h.autono(+) AND H.DOCCD = I.DOCCD(+) and h.compcd=j.compcd(+) and h.loccd=j.loccd(+) ";
                Str += "AND H.DOCDT <=  to_date('" + docdt + "','dd/mm/yyyy')  ";
                Str += "AND B.DRCR = 'D' ";
                if (skipautono.retStr() != "") Str += "AND A.AUTONO NOT IN (" + skipautono + ") ";  
                if (compcd.retStr() != "") Str += "AND H.COMPCD||H.LOCCD IN(" + compcd + ") "; else Str += "AND h.compcd = '" + COM + "'  and h.loccd = '" + LOC + "' ";
                if (invest.retStr() != "") Str += "AND B.INVEST='" + invest + "'  ";
                if (ctgcd.retStr() != "") Str += "AND C.CTGCD IN(" + ctgcd + ")  ";
                if (shrcd.retStr() != "") Str += "AND C.SHRCD IN(" + shrcd + ")  ";
                if (shrdpcd.retStr() != "") Str += "AND B.SHRDPCD IN(" + shrdpcd + ")  ";
                Str += "ORDER BY COMPNM, CTGNM, SHRNM, DOCDT, TRADEDT, AUTONO, SLNO ";
                DataTable Rs_In = SQLquery(Str);

                Str = "  SELECT distinct F.COMPCD, A.AUTONO, F.DOCDT, A.SLCD, E.SLNM,F.DOCNO, F.COMPCD||F.LOCCD||B.SHRCD COMPSHRCD, B.SHRDPCD,F.COMPCD||F.loccd comploccd , ";
                Str += "NVL(B.TRADEDT, F.DOCDT)TRADEDT, B.SLNO, B.SHRCD, B.QTY, B.COSTAMT, B.RATE, C.SHRNM,D.CTGNM, C.SHRSECT ";
                Str += " FROM " + scmf + ".T_SHRTXN A, " + scmf + ".T_SHRTXNDTL B, " + scmf + ".M_SHRMST C, " + scmf + ".M_SHRCTG D, " + scmf + ".M_SUBLEG E, ";
                Str += scmf + ".T_CNTRL_HDR f, " + scmf + ".M_DOCTYPE G ";
                Str += "WHERE A.AUTONO = B.AUTONO AND B.SHRCD = C.SHRCD AND C.CTGCD = D.CTGCD AND A.SLCD = E.SLCD(+) and a.autono = F.autono(+) AND F.DOCCD = G.DOCCD(+) ";
                Str += "AND F.DOCDT <= to_date('" + docdt + "','dd/mm/yyyy')  AND B.DRCR = 'C' ";
                if (skipautono.retStr() != "") Str += "AND A.AUTONO NOT IN (" + skipautono + ") ";
                if (compcd.retStr() != "") Str += "AND F.COMPCD||F.LOCCD IN(" + compcd + ") "; else Str += "AND f.compcd = '" + COM + "'  and f.loccd = '" + LOC + "' ";
                if (invest.retStr() != "") Str += "AND B.INVEST='" + invest + "'  ";
                if (ctgcd.retStr() != "") Str += "AND C.CTGCD IN(" + ctgcd + ")  ";
                if (shrcd.retStr() != "") Str += "AND C.SHRCD IN(" + shrcd + ")  ";
                if (shrdpcd.retStr() != "") Str += "AND B.SHRDPCD IN(" + shrdpcd + ")  ";
                Str += "ORDER BY COMPSHRCD, CTGNM, SHRNM, DOCDT, TRADEDT, AUTONO, SLNO ";
                DataTable Rs_Out = SQLquery(Str);

                DataTable fixRs = new DataTable("stock");
                fixRs.Columns.Add("COMPSHRCD", typeof(string), "");
                //fixRs.Columns.Add("COMPCD", typeof(string), "");
                //fixRs.Columns.Add("COMPNM", typeof(string), "");
                fixRs.Columns.Add("comploccd", typeof(string), "");
                fixRs.Columns.Add("complocnm", typeof(string), "");
                fixRs.Columns.Add("CTGCD", typeof(string), "");
                fixRs.Columns.Add("CTGNM", typeof(string), "");
                fixRs.Columns.Add("SHRCD", typeof(string), "");
                fixRs.Columns.Add("SHRNM", typeof(string), "");
                fixRs.Columns.Add("SHRSECT", typeof(string), "");
                fixRs.Columns.Add("QUOTED", typeof(string), "");
                fixRs.Columns.Add("FACEVAL", typeof(double), "");
                fixRs.Columns.Add("AUTONO", typeof(string), "");
                fixRs.Columns.Add("INVEST", typeof(string), "");
                fixRs.Columns.Add("SLNO", typeof(double), "");
                fixRs.Columns.Add("DOCDT", typeof(string), "");
                fixRs.Columns.Add("TRADEDT", typeof(string), "");
                fixRs.Columns.Add("SLCD", typeof(string), "");
                fixRs.Columns.Add("SLNM", typeof(string), "");
                fixRs.Columns.Add("FALSEREC", typeof(string), "");
                fixRs.Columns.Add("RECNO", typeof(string), "");
                fixRs.Columns.Add("SHRDPCD", typeof(string), "");
                fixRs.Columns.Add("SHRDPNM", typeof(string), "");
                fixRs.Columns.Add("QTY", typeof(double), "");
                fixRs.Columns.Add("RATE", typeof(double), "");
                fixRs.Columns.Add("COSTAMT", typeof(double), "");
                fixRs.Columns.Add("OUTDT", typeof(string), "");
                fixRs.Columns.Add("OUTNO", typeof(string), "");
                fixRs.Columns.Add("OUTAUTONO", typeof(string), "");
                fixRs.Columns.Add("OUTSL_CD", typeof(string), "");
                fixRs.Columns.Add("OUTSL_NM", typeof(string), "");
                fixRs.Columns.Add("OUTQNTY", typeof(double), "");
                fixRs.Columns.Add("OUTRATE", typeof(double), "");
                fixRs.Columns.Add("OUTAMT", typeof(double), "");
                fixRs.Columns.Add("BALQNTY", typeof(double), "");
                fixRs.Columns.Add("BALAMT", typeof(double), "");
                fixRs.Columns.Add("SHRSTAMT", typeof(double), "");
                fixRs.Columns.Add("SHRLTAMT", typeof(double), "");

                string Compcd, CompNm, comploccd, complocnm, ShrCd, ShrNm, TradeDt, CompShrCd, CtgCd, CtgNm, Shrsect, Quoted, ShrdpCd, ShrdpNm, Invest, RecNo;
                int lnNo;
                Double SalQty, BalQnty, AvRate = 0, crSalAmt, BalAmt, xx, ProfAmt;
                DateTime Docdt;

                lnNo = 0;
                int maxR = Rs_In.Rows.Count - 1, i = 0, rNo = 0;
                while (i <= maxR)
                {
                    //Compcd = Rs_In.Rows[i]["COMPCD"].retStr();
                    //CompNm = Rs_In.Rows[i]["COMPNM"].retStr();
                    comploccd = Rs_In.Rows[i]["comploccd"].retStr();
                    complocnm = Rs_In.Rows[i]["complocnm"].retStr();
                    while (Rs_In.Rows[i]["comploccd"].retStr() == comploccd)
                    {
                        ShrCd = Rs_In.Rows[i]["SHRCD"].retStr();
                        ShrNm = Rs_In.Rows[i]["SHRNM"].retStr();
                        CtgCd = Rs_In.Rows[i]["CTGCD"].retStr();
                        CtgNm = Rs_In.Rows[i]["CTGNM"].retStr();
                        CompShrCd = comploccd + ShrCd;
                        ShrdpCd = Rs_In.Rows[i]["SHRDPCD"].retStr();
                        ShrdpNm = Rs_In.Rows[i]["SHRDPNM"].retStr();
                        Invest = Rs_In.Rows[i]["INVEST"].retStr();
                        Shrsect = Rs_In.Rows[i]["SHRSECT"].retStr();
                        int fixstartno = -1;
                        //Quoted = Rs_In.Rows[i]["QUOTED"].retStr() == "Y" ? "Quoted" : "Unquoted";
                        while (Rs_In.Rows[i]["comploccd"].retStr() == comploccd && Rs_In.Rows[i]["SHRCD"].retStr() == ShrCd)
                        {
                            lnNo = lnNo + 1;
                            fixRs.Rows.Add(""); rNo = fixRs.Rows.Count - 1;
                            if (fixstartno == -1) fixstartno = rNo;
                            fixRs.Rows[rNo]["COMPSHRCD"] = Rs_In.Rows[i]["comploccd"].retStr() + Rs_In.Rows[i]["SHRCD"].retStr();
                            fixRs.Rows[rNo]["comploccd"] = Rs_In.Rows[i]["comploccd"].retStr();
                            fixRs.Rows[rNo]["complocnm"] = Rs_In.Rows[i]["complocnm"].retStr();
                            fixRs.Rows[rNo]["CTGCD"] = Rs_In.Rows[i]["CTGCD"].retStr();
                            fixRs.Rows[rNo]["CTGNM"] = Rs_In.Rows[i]["CTGNM"].retStr();
                            fixRs.Rows[rNo]["SHRCD"] = Rs_In.Rows[i]["SHRCD"].retStr();
                            fixRs.Rows[rNo]["SHRNM"] = Rs_In.Rows[i]["SHRNM"].retStr();
                            //fixRs.Rows[rNo]["FACEVAL"] = Rs_In.Rows[i]["FACEVAL"].retStr();
                            fixRs.Rows[rNo]["SHRSECT"] = Shrsect;
                            //fixRs.Rows[rNo]["QUOTED"] = Quoted;
                            fixRs.Rows[rNo]["autoNO"] = Rs_In.Rows[i]["autoNO"].retStr();
                            fixRs.Rows[rNo]["INVEST"] = Rs_In.Rows[i]["INVEST"].retStr();
                            fixRs.Rows[rNo]["SlNo"] = Rs_In.Rows[i]["SlNo"].retStr();
                            fixRs.Rows[rNo]["DOCDT"] = Rs_In.Rows[i]["DOCDT"].retStr().Remove(10);
                            fixRs.Rows[rNo]["TRADEDT"] = Rs_In.Rows[i]["TRADEDT"].retStr().Remove(10);
                            fixRs.Rows[rNo]["SLCD"] = Rs_In.Rows[i]["SLCD"].retStr();
                            fixRs.Rows[rNo]["SLNM"] = Rs_In.Rows[i]["SLNM"].retStr();
                            fixRs.Rows[rNo]["SHRDPCD"] = Rs_In.Rows[i]["SHRDPCD"].retStr();
                            fixRs.Rows[rNo]["SHRDPNM"] = Rs_In.Rows[i]["SHRDPNM"].retStr();
                            fixRs.Rows[rNo]["QTY"] = Rs_In.Rows[i]["QTY"].retDbl();
                            fixRs.Rows[rNo]["Rate"] = Rs_In.Rows[i]["Rate"].retDbl();
                            fixRs.Rows[rNo]["COSTAMT"] = Rs_In.Rows[i]["COSTAMT"].retDbl();
                            //fixRs.Rows[rNo]["RECno"] = "PR" + REPL("0", 6 - Len(CStr(lnNo))) + CStr(lnNo)
                            fixRs.Rows[rNo]["BALQNTY"] = fixRs.Rows[rNo]["QTY"].retDbl();
                            fixRs.Rows[rNo]["BALAMT"] = fixRs.Rows[rNo]["COSTAMT"].retDbl();
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        if (Rs_Out.Rows.Count > 0)
                        {
                            rNo = fixstartno;
                            string sel1 = "COMPSHRCD='" + CompShrCd + "'";
                            var rm1 = Rs_Out.Select(sel1);
                            int j = 0;
                            while (j <= rm1.Count() - 1)
                            {
                                SalQty = rm1[j]["QTY"].retDbl();
                                crSalAmt = rm1[j]["COSTAMT"].retDbl();
                                DataView dv = fixRs.DefaultView;
                                dv.Sort = "COMPSHRCD,RECNO";
                                fixRs = dv.ToTable();
                                sel1 = "COMPSHRCD='" + CompShrCd + "'";
                                var rm2 = fixRs.Select(sel1);
                                while (SalQty > 0 && rm2.Count() > 0)
                                {
                                    int k = 0;
                                    while (k <= rm2.Count() - 1)
                                    {
                                        if (rm2[k]["BALQNTY"].retDbl() != 0)
                                        {
                                            if (rm2[k]["BALQNTY"].retDbl() >= SalQty || rm2[k]["OUTQNTY"].retDbl() != 0)
                                            {
                                                if (rm2[k]["OUTQNTY"].retDbl() != 0)
                                                {
                                                    RecNo = rm2[k]["RECno"].retStr();
                                                    BalQnty = rm2[k]["BALQNTY"].retDbl();
                                                    BalAmt = rm2[k]["BALAMT"].retDbl();
                                                    Docdt = Convert.ToDateTime(rm2[k]["DOCDT"].retStr());
                                                    TradeDt = rm2[k]["TRADEDT"].retDateStr().Substring(0, 10);
                                                    CtgCd = rm2[k]["CTGCD"].retStr();
                                                    CtgNm = rm2[k]["CTGNM"].retStr();

                                                    //fixRs.Rows[rNo]["BALQNTY"] = 0;
                                                    //fixRs.Rows[rNo]["BALAMT"] = 0;
                                                    rm2[k]["BALQNTY"] = 0;
                                                    rm2[k]["BALAMT"] = 0;

                                                    fixRs.Rows.Add(""); rNo = fixRs.Rows.Count - 1;
                                                    fixRs.Rows[rNo]["comploccd"] = comploccd;
                                                    fixRs.Rows[rNo]["complocnm"] = complocnm;
                                                    fixRs.Rows[rNo]["SHRCD"] = ShrCd;
                                                    fixRs.Rows[rNo]["SHRNM"] = ShrNm;
                                                    fixRs.Rows[rNo]["SHRSECT"] = Shrsect;
                                                    fixRs.Rows[rNo]["CTGCD"] = CtgCd;
                                                    fixRs.Rows[rNo]["CTGNM"] = CtgNm;
                                                    fixRs.Rows[rNo]["COMPSHRCD"] = CompShrCd;
                                                    fixRs.Rows[rNo]["SHRDPCD"] = ShrdpCd;
                                                    fixRs.Rows[rNo]["SHRDPNM"] = ShrdpNm;
                                                    fixRs.Rows[rNo]["INVEST"] = Invest;
                                                    fixRs.Rows[rNo]["DOCDT"] = Docdt.retDateStr();
                                                    fixRs.Rows[rNo]["TRADEDT"] = TradeDt.retDateStr();
                                                    fixRs.Rows[rNo]["RECno"] = RecNo;
                                                    fixRs.Rows[rNo]["FALSEREC"] = "Y";
                                                    fixRs.Rows[rNo]["BALQNTY"] = BalQnty;
                                                    fixRs.Rows[rNo]["BALAMT"] = BalAmt;
                                                }
                                                if (rm2[k]["BALQNTY"].retDbl() >= SalQty)
                                                {
                                                    if (rm2[k]["BALQNTY"].retDbl() == 0)
                                                    {
                                                        AvRate = 0;
                                                    }
                                                    else
                                                    {
                                                        AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                                                    }

                                                    rm2[k]["BALQNTY"] = rm2[k]["BALQNTY"].retDbl() - SalQty;
                                                    rm2[k]["BALAMT"] = (rm2[k]["BALQNTY"].retDbl() * AvRate).toRound(2);
                                                    rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retStr().Remove(10);
                                                    rm2[k]["OUTNO"] = rm1[j]["DOCNO"].retStr();
                                                    rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"].retStr();
                                                    rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"].retStr();
                                                    rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"].retStr();//Left(FIlc(rm1[j]["SLNM), 8)
                                                    rm2[k]["OUTQNTY"] = SalQty;
                                                    rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                                                    rm2[k]["OUTAMT"] = crSalAmt;
                                                    //'Short / Long Term
                                                    if (rm2[k]["OUTQNTY"].retDbl() == rm2[k]["QTY"].retDbl())
                                                    {
                                                        ProfAmt = rm2[k]["OUTAMT"].retDbl() - rm2[k]["COSTAMT"].retDbl();
                                                    }
                                                    else {
                                                        ProfAmt = rm2[k]["OUTAMT"].retDbl() - (AvRate * rm2[k]["OUTQNTY"].retDbl()).toRound(2);
                                                    }
                                                    if ((Convert.ToDateTime(rm2[k]["OUTDT"].retStr()).Date - Convert.ToDateTime(rm2[k]["TRADEDT"].retStr()).Date).TotalDays <= 365)
                                                    {
                                                        rm2[k]["SHRSTAMT"] = ProfAmt;
                                                    }
                                                    else
                                                    {
                                                        rm2[k]["SHRLTAMT"] = ProfAmt;
                                                    }
                                                    SalQty = 0;
                                                    crSalAmt = 0;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (rm2[k]["BALQNTY"].retDbl() == 0)
                                                    {
                                                        AvRate = 0;
                                                    }
                                                    else
                                                    {
                                                        AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                                                    }
                                                    rm2[k]["BALAMT"] = 0;
                                                    rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retStr().Remove(10);
                                                    rm2[k]["OUTNO"] = rm1[j]["DOCNO"].retStr();
                                                    rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"].retStr();
                                                    rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"].retStr();
                                                    rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"].retStr();// Left(FIlc(rm1[j]["SLNM), 8)
                                                    rm2[k]["OUTQNTY"] = rm2[k]["BALQNTY"].retDbl();
                                                    rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                                                    xx = (crSalAmt / SalQty).toRound(4);
                                                    xx = (xx * rm2[k]["BALQNTY"].retDbl()).toRound(2);
                                                    crSalAmt = crSalAmt - xx;
                                                    SalQty = SalQty - rm2[k]["BALQNTY"].retDbl();
                                                    rm2[k]["OUTAMT"] = xx;
                                                    rm2[k]["BALQNTY"] = 0;
                                                    //'Short / Long Term
                                                    if (rm2[k]["OUTQNTY"].retDbl() == rm2[k]["QTY"].retDbl())
                                                    {
                                                        ProfAmt = rm2[k]["OUTAMT"].retDbl() - rm2[k]["COSTAMT"].retDbl();
                                                    }
                                                    else {
                                                        ProfAmt = rm2[k]["OUTAMT"].retDbl() - (AvRate * rm2[k]["OUTQNTY"].retDbl()).toRound(2);
                                                    }
                                                    if ((Convert.ToDateTime(rm2[k]["OUTDT"].retStr()).Date - Convert.ToDateTime(rm2[k]["TRADEDT"].retStr()).Date).TotalDays <= 365)
                                                    {
                                                        rm2[k]["SHRSTAMT"] = ProfAmt;
                                                    }
                                                    else
                                                    {
                                                        rm2[k]["SHRLTAMT"] = ProfAmt;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (rm2[k]["BALQNTY"].retDbl() == 0)
                                                {
                                                    AvRate = 0;
                                                }
                                                else
                                                {
                                                    AvRate = (rm2[k]["BALAMT"].retDbl() / rm2[k]["BALQNTY"].retDbl()).toRound(4);
                                                }

                                                rm2[k]["BALAMT"] = 0;
                                                rm2[k]["OUTDT"] = rm1[j]["DOCDT"].retStr().Remove(10);
                                                rm2[k]["OUTNO"] = rm1[j]["DOCNO"];
                                                rm2[k]["OUTAUTONO"] = rm1[j]["autoNO"];
                                                rm2[k]["OUTSL_CD"] = rm1[j]["SLCD"];
                                                rm2[k]["OUTSL_NM"] = rm1[j]["SLNM"];// Left(FIlc(rm1[j]["SLNM"]), 8)
                                                rm2[k]["OUTQNTY"] = rm2[k]["BALQNTY"].retDbl(); // rm2[k]["BALQNTY"].retDbl();
                                                rm2[k]["OUTRATE"] = rm1[j]["Rate"].retDbl();
                                                xx = (crSalAmt / SalQty).toRound(4);
                                                xx = (xx * rm2[k]["BALQNTY"].retDbl()).toRound(2);
                                                crSalAmt = crSalAmt - xx;
                                                SalQty = SalQty - rm2[k]["BALQNTY"].retDbl();
                                                rm2[k]["OUTAMT"] = xx;
                                                rm2[k]["BALQNTY"] = 0;
                                                //'Short / Long Term
                                                if (rm2[k]["OUTQNTY"].retDbl() == rm2[k]["QTY"].retDbl())
                                                {
                                                    ProfAmt = rm2[k]["OUTAMT"].retDbl() - rm2[k]["COSTAMT"].retDbl();
                                                }
                                                else {
                                                    ProfAmt = rm2[k]["OUTAMT"].retDbl() - (AvRate * rm2[k]["OUTQNTY"].retDbl()).toRound(2);
                                                }
                                                if ((Convert.ToDateTime(rm2[k]["OUTDT"].retStr()).Date - Convert.ToDateTime(rm2[k]["TRADEDT"].retStr()).Date).TotalDays <= 365)
                                                {
                                                    rm2[k]["SHRSTAMT"] = ProfAmt;
                                                }
                                                else {
                                                    rm2[k]["SHRLTAMT"] = ProfAmt;
                                                }
                                            }
                                        }
                                        k = k + 1;
                                        if (k > rm2.Count() - 1) break;
                                        if (rm2[k]["COMPSHRCD"].retStr() != CompShrCd) break;
                                    }
                                }
                                j = j + 1;
                                if (j > rm1.Count() - 1) break;
                                if (rm1[j]["COMPSHRCD"].retStr() != CompShrCd) break;
                            }
                        }
                        if (i > maxR) break;
                    }
                }

                if (OnlyBal == true && fixRs.Rows.Count > 0)
                {
                    fixRs = fixRs.AsEnumerable().Where(r => r.Field<double>("BALQNTY") != 0).CopyToDataTable();
                }
                DataView dv1 = fixRs.DefaultView;
                dv1.Sort = "complocnm,CTGNM,SHRNM,RECNO";
                fixRs = dv1.ToTable();

                return fixRs;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
    }
}