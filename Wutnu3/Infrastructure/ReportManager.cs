using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace Wutnu.Infrastructure
{
    /// <summary>
    /// https://azure.microsoft.com/en-us/documentation/articles/virtual-machines-windows-classic-sql-server-reportviewer/
    /// https://msdn.microsoft.com/library/bb885185.aspx
    /// </summary>
    public class ReportManager
    {
        public readonly HttpContextBase Htx = null;
        public string ReportPath;
        public string MimeType;
        public string ReportType;
        public string FileName;

        public ReportManager(HttpContextBase httpContext)
        {
            Htx = httpContext;
        }

        public byte[] GetReport(Stream reportDef, string reportName, SqlDataReader reader, Dictionary<string, string> parms)
        {
            try
            {
                var localReport = new LocalReport();

                localReport.LoadReportDefinition(reportDef);

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
                    "  <PageWidth>8.5in</PageWidth>" +
                    "  <PageHeight>11in</PageHeight>" +
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
            catch (Exception ex)
            {
                throw new Exception("Error rendering custom report", ex);
            }
        }
        public string GetReportExtension()
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
        public void WriteOut(byte[] renderedBytes)
        {
            //Clear the response stream and write the bytes to the outputstream
            //Set content-disposition to "attachment" so that user is prompted to take an action
            //on the file (open or save)
            var res = Htx.Response;
            res.Clear();
            res.Buffer = true;
            res.CacheControl = "no-cache";
            res.ContentType = MimeType;
            res.AddHeader("expires", "0");
            res.AddHeader("Content-Length", renderedBytes.Length.ToString());
            res.AddHeader("content-disposition", "attachment; filename=" + FileName);
            res.BinaryWrite(renderedBytes);
            res.Flush();
            //res.Close();
        }
    }
}
