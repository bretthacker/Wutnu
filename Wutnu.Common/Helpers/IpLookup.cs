using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Wutnu.Common.Helpers
{
    public static class IpLookup
    {
        public static ExtremeIpResult Get(string ip)
        {

            using (var web = new WebClient())
            {
                var url = string.Format("http://extreme-ip-lookup.com/json/{0}", ip);
                var res = web.DownloadString(url);
                return JsonConvert.DeserializeObject<ExtremeIpResult>(res);
            }
        }
    }

    public class ExtremeIpResult
    {
        [JsonProperty(PropertyName = "businessName")]
        public string BusinessName { get; set; }

        [JsonProperty(PropertyName = "businessWebsite")]
        public string BusinessWebsite { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "continent")]
        public string Continent { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "ipName")]
        public string IpName { get; set; }

        [JsonProperty(PropertyName = "ipType")]
        public string IpType { get; set; }

        [JsonProperty(PropertyName = "isp")]
        public string Isp { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public string Latitude { get; set; }

        [JsonProperty(PropertyName = "lon")]
        public string Longitude { get; set; }

        [JsonProperty(PropertyName = "org")]
        public string Org { get; set; }

        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}
