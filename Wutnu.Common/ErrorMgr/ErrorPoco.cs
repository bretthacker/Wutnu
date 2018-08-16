namespace Wutnu.Common.ErrorMgr
{
    public class ErrorPoco
    {
        public int ErrorId { get; set; }
        public System.DateTime ErrorDate { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserComment { get; set; }
        public string Status { get; set; }
        public string URI { get; set; }
        public string SiteURL { get; set; }
        public string ValidationErrors { get; set; }
        public string Message { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionSource { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string QSData { get; set; }
        public string PostData { get; set; }
        public string Referrer { get; set; }
        public string UserId { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
    }
}
