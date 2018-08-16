using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wutnu.Web.Business
{
    public class OIDConfigResponse
    {
        [JsonProperty(PropertyName = "authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonProperty(PropertyName = "token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonProperty(PropertyName = "token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; }

        [JsonProperty(PropertyName = "jwks_uri")]
        public string JwksUri { get; set; }

        [JsonProperty(PropertyName = "response_modes_supported")]
        public string[] ResponseModesSupported { get; set; }

        [JsonProperty(PropertyName = "subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; }

        [JsonProperty(PropertyName = "id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        [JsonProperty(PropertyName = "http_logout_supported")]
        public bool HttpLogoutSupported { get; set; }

        [JsonProperty(PropertyName = "frontchannel_logout_supported")]
        public bool FrontchannelLogoutSupported { get; set; }

        [JsonProperty(PropertyName = "end_session_endpoint")]
        public string EndSessionEndpoint { get; set; }

        [JsonProperty(PropertyName = "response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        [JsonProperty(PropertyName = "scopes_supported")]
        public string[] ScopesSupported { get; set; }

        [JsonProperty(PropertyName = "issuer")]
        public string Issuer { get; set; }

        [JsonProperty(PropertyName = "claims_supported")]
        public string[] ClaimsSupported { get; set; }

        [JsonProperty(PropertyName = "microsoft_multi_refresh_token")]
        public string MicrosoftMultiRefreshToken { get; set; }

        [JsonProperty(PropertyName = "check_session_iframe")]
        public string CheckSessionIframe { get; set; }

        [JsonProperty(PropertyName = "userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; }

        [JsonProperty(PropertyName = "tenant_region_scope")]
        public string TenantRegionScope { get; set; }

        [JsonProperty(PropertyName = "cloud_instance_name")]
        public string CloudInstanceName { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }

    public class OIDConfigError
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "error_codes")]
        public string[] ErrorCodes { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty(PropertyName = "trace_id")]
        public string TraceID { get; set; }

        [JsonProperty(PropertyName = "correlation_id")]
        public string CorrelationId { get; set; }
    }
}
