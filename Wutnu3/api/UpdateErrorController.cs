using System;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wutnu.Infrastructure;
using Wutnu.Common.ErrorMgr;
using Wutnu.Common;
using Wutnu.Data;

namespace Wutnu.api
{
    public class ErrUpdatePoco
    {
        public string Comment { get; set; }
        public string Error { get; set; }
        public int Id { get; set; }
    }

    public class UpdateErrorController : BaseApiController
    {
        public UpdateErrorController(WutCache cache, WutNuContext models)
        : base(cache, models)
        {
        }

        // POST api/updateerror
        public void Post(ErrUpdatePoco err)
        {
            try
            {
                try
                {
                    if (err.Id == 0)
                    {
                        var cliError = JsonConvert.DeserializeObject<dynamic>(err.Error);
                        var errorString = new StringBuilder();
                        foreach (JProperty item in cliError)
                            errorString.AppendFormat("{0}: {1}<br>", item.Name, item.Value.ToString().Replace("\n", "<br>\n"));

                        var innerEx = new Exception(errorString.ToString()) {Source = "Javascript (client)"};
                        Logging.WriteDebugInfoToErrorLog("A client-side error occured", innerEx, Wutcontext, WebContextBase, err.Comment);
                    }
                    else
                    {
                        using (var errbl = new ErrorItemBL(WebContextBase, Wutcontext))
                        {
                            errbl.UpdateErrorItemUserComment(err.Id, err.Comment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Writing to node WEL
                    Logging.WriteToAppLog("Unable to save user comments. \r\nError: " + ex.Message + ". \r\nComment: " + err.Comment, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception)
            {
                //not a biggie, we have to pick our battles here...
                HttpContext.Current.ClearError();
            }
        }
    }
}
