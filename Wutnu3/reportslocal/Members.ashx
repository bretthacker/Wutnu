<%@ WebHandler Language="C#" Class="MemberAccounting" %>

using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using Microsoft.Reporting.WebForms;

public class MemberAccounting : IHttpHandler {

    private string ReportPath;
    private string mimeType;
    private string fileName;
    private HttpContext hcon;
    private string ProfileType, Company;

    public void ProcessRequest (HttpContext context) {
        hcon = context;
        var req = hcon.Request;
        ReportPath = hcon.Server.MapPath("~/admin/Reports/Members.rdlc");
        fileName = "MembershipReport.pdf";
        
        ProfileType = req.QueryString["pt"];
        Company = req.QueryString["rc"];
        
        WriteOut(GetMemberReport());
    }

    protected SqlDataReader GetMemberData()
    {
        string sConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TRCConnectionString"].ToString();
        SqlConnection conn = new SqlConnection(sConnectionString);
        conn.Open();
        SqlCommand comm = new SqlCommand();
        comm.Connection = conn;
        comm.CommandTimeout = 60;
        comm.CommandType = CommandType.StoredProcedure;
        comm.CommandText = "usp_rpt_Membership";
        comm.Parameters.Add(new SqlParameter("ProfileType", ProfileType));
        comm.Parameters.Add(new SqlParameter("Company", Company));
        return comm.ExecuteReader();
    }
    protected byte[] GetMemberReport()
    {
        LocalReport localReport = new LocalReport();
        localReport.ReportPath = ReportPath;
        ReportDataSource ds = new ReportDataSource("Membership", GetMemberData());
        localReport.DataSources.Add(ds);
        
        ReportParameterCollection ReportParms = new ReportParameterCollection();
        ReportParms.Add(new ReportParameter("ProfileType", ProfileType, false));
        localReport.SetParameters(ReportParms);

        string reportType = "PDF";
        string encoding;
        string fileNameExtension;
        //The DeviceInfo settings should be changed based on the reportType
        //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
        string deviceInfo = "<DeviceInfo>" +
            "  <OutputFormat>PDF</OutputFormat>" +
            "  <PageWidth>11in</PageWidth>" +
            "  <PageHeight>8.5in</PageHeight>" +
            "  <MarginTop>0.5in</MarginTop>" +
            "  <MarginLeft>0.5in</MarginLeft>" +
            "  <MarginRight>0.5in</MarginRight>" +
            "  <MarginBottom>0.5in</MarginBottom>" +
            "</DeviceInfo>";
        Warning[] warnings;
        string[] streams;

        //Render the report
        return localReport.Render(
            reportType,
            deviceInfo,
            out mimeType,
            out encoding,
            out fileNameExtension,
            out streams,
            out warnings);
    }

    protected void WriteOut(byte[] renderedBytes)
    {
        //Clear the response stream and write the bytes to the outputstream
        //Set content-disposition to "attachment" so that user is prompted to take an action
        //on the file (open or save)
        var res = hcon.Response;
        res.Clear();
        res.Buffer = true;
        res.CacheControl = "no-cache";
        res.ContentType = mimeType;
        res.AddHeader("expires", "0");
        res.AddHeader("Content-Length", renderedBytes.Length.ToString());
        res.AddHeader("content-disposition", "attachment; filename=" + fileName);
        res.BinaryWrite(renderedBytes);
        res.Flush();
        res.Close();
        res.End();
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}
