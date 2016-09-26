<%@ WebHandler Language="C#" Class="AllVars" %>

using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.SessionState;

public class AllVars : IHttpHandler, IRequiresSessionState
{
	private StringBuilder sOut = new StringBuilder();
	private HttpContext _context;

	public void ProcessRequest (HttpContext context)
	{
		_context=context;

		sOut.AppendLine("<html><head><title>All Variables</title><style type='text/css'>td {font:9pt verdana;} th {font:10pt verdana; color:white; background-color:teal;font-weight:bold}</style></head><body>");
		sOut.AppendFormat("Time: {0}<br>", System.DateTime.Now.ToString());
		sOut.AppendLine("<div style='width:900px'><table border='1'><tr><th colspan='2'>Application Variables</th></tr>");

		GetAppVars();

		sOut.AppendLine("</table><table border='1'><tr><th colspan='2'>Session Variables</th></tr>");

		GetSessionVars();

		sOut.AppendLine("</table><table border='1'><tr><th colspan='2'>Header Variables</th></tr>");

		GetHeaderVars();

		sOut.AppendLine("</table><hr/><table border='1'><tr><th colspan='2'>Cookie Variables</th></tr>");

		GetCookieVars();

		sOut.AppendLine("</table><hr/><table border='1'><tr><th colspan='2'>Server Variables</th></tr>");

		GetServerVars();

		sOut.AppendLine("</table></div></body></html>");

		context.Response.Write(sOut.ToString());
	}

	private void GetHeaderVars()
	{
		foreach(string x in _context.Request.Headers) {
			sOut.AppendLine("<tr>");
			sOut.AppendLine("<td><b>" + x + "</b>:</td>");
			sOut.AppendLine("<td>" + _context.Request.Headers[x].ToString() + "</td>");
 			sOut.AppendLine("</tr>");
		}
	}
	private void GetAppVars()
	{
		foreach(string x in _context.Application.Contents) {
			sOut.AppendLine("<tr>");
			sOut.AppendLine("<td><b>" + x + "</b>:</td>");
			sOut.AppendLine("<td>" + _context.Application.Contents[x].ToString() + "</td>");
 			sOut.AppendLine("</tr>");
		}
	}
	private void GetSessionVars()
	{
		foreach (string x in _context.Session.Contents) {
			sOut.AppendLine("<tr>");
			sOut.AppendLine("<td><B>" + x + "</b>:</td>");
			var oItem = _context.Session.Contents[x];
			sOut.AppendLine("<td>" + oItem.ToString() + "</td>");
			sOut.AppendLine("</tr>");
		}
	}
	private void GetCookieVars()
	{
		foreach (string y in _context.Request.Cookies)
		{
			sOut.AppendLine("<tr>");
			sOut.AppendLine("<td><b>" + y + "</b>:</td>");
			sOut.AppendLine("<td>" + _context.Request.Cookies[y].Value.ToString() + "</td>");
			sOut.AppendLine("</tr>");
		}
	}
	private void GetServerVars()
	{
		foreach (string var in _context.Request.ServerVariables)
		{
			sOut.AppendLine("<tr>");
			sOut.AppendLine("<td><b>" + var + "</B>:</td>");
			sOut.AppendLine("<td>" + _context.Request.ServerVariables[var].ToString() + "</td>");
			sOut.AppendLine("</tr>");
		}
	}
	public bool IsReusable {
        	get {
        		return false;
		}
	}

}
