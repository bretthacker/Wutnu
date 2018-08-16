using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;

namespace Wutnu.Areas.Manage.Views.Reports
{
    public abstract class Index: WebViewPage 
    {
        protected string ReportPath;
        protected string MimeType;
        protected string ReportType;
        protected string FileName;
        protected HttpContext Hcon;

        protected byte[] GetReport(string reportName, SqlDataReader reader, Dictionary<string, string> parms)
        {
            var localReport = new LocalReport
                {
                    ReportPath = ReportPath
                };
            var ds = new ReportDataSource(reportName, reader);
            localReport.DataSources.Add(ds);

            var reportParms = new ReportParameterCollection();
            foreach (var parm in parms)
            {
                reportParms.Add(new ReportParameter(parm.Key, parm.Value, false));
            }

            localReport.SetParameters(reportParms);

            string encoding;
            string fileNameExtension;
            //The DeviceInfo settings should be changed based on the reportType
            //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            string deviceInfo = "<DeviceInfo>" +
                "  <OutputFormat>" + ReportType + "</OutputFormat>" +
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
                ReportType,
                deviceInfo,
                out MimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);
        }
        protected string GetReportExtension()
        {
            string sOut = "";
            switch (ReportType)
            {
                case "PDF":
                    sOut = ".pdf";
                    break;
                case "Excel":
                    sOut = ".xls";
                    break;
            }
            return sOut;
        }
        protected void WriteOut(byte[] renderedBytes)
        {
            //Clear the response stream and write the bytes to the outputstream
            //Set content-disposition to "attachment" so that user is prompted to take an action
            //on the file (open or save)
            var res = Hcon.Response;
            res.Clear();
            res.Buffer = true;
            res.CacheControl = "no-cache";
            res.ContentType = MimeType;
            res.AddHeader("expires", "0");
            res.AddHeader("Content-Length", renderedBytes.Length.ToString());
            res.AddHeader("content-disposition", "attachment; filename=" + FileName);
            res.BinaryWrite(renderedBytes);
            res.Flush();
            res.Close();
            res.End();
        }
    }
}