using LLS.Lib.Extensions;
using LLS.Lib.Json;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib
{
    public class UserData
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("ip")]
        public string IP { get; set; }
        [JsonProperty("is_active")]
        public bool IsActive { get {
                return License.ExpiresInSeconds > 0 || License.IsLifetime;
            } }
        [JsonProperty("createdat")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("license")]
        public UserLicense License { get; set; }
    }
    public class UserLicense
    {
        [JsonProperty("runs_out_at")]
        public DateTime ExpiresAt { get; set; }
        [JsonProperty("runs_out_seconds")]
        public int? ExpiresInSeconds { get; set; } = null;
        [JsonProperty("level")]
        public int Level { get; set; } = 1;
        [JsonProperty("lifetime")]
        public bool IsLifetime { get; set; } = false;
    }
    public class IPInfo
    {
        public bool IsValid { get; set; }
        [JsonProperty("ip")]
        public string IPAddress { get; set; }
        [JsonProperty("hostname")]
        public string Hostname { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("region_code")]
        public string RegionCode { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("continent")]
        public string Continent { get; set; }
        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }
        [JsonProperty("postal")]
        public string Postal { get; set; }
        [JsonProperty("offset")]
        public int Offset { get; set; }
        [JsonProperty("isp")]
        public string ISP { get; set; }
        [JsonProperty("asn")]
        public string Asn { get; set; }
        [JsonProperty("asn_org")]
        public string AsnOrg { get; set; }
        [JsonProperty("org")]
        public string Org { get; set; }
        /// <summary>
        /// Simple Constructor to build the IPInfo Object
        /// </summary>
        public IPInfo() { this.IsValid = false; }
        /// <summary>
        /// Creates Object with IP Data, NULL equals your current Data
        /// </summary>
        /// <param name="hostname">Ex:. www.google.com, 127.0.0.1, NULL(Returns your current Data)</param>
        public IPInfo getHost(string hostname = null)
        {
            RestClient rc = new RestClient("http://host.sh");
            RestRequest rr = new RestRequest(hostname == null ? "json" : "{hostname}/json", Method.GET);
            if(hostname != null) rr.AddUrlSegment("hostname", hostname);
            var r = rc.Execute(rr);
            if (Debugger.IsAttached) Debug.WriteLine("IPINFO: " + r.ResponseUri + Environment.NewLine + "Response: " + r.Content);
            if(r.StatusCode == HttpStatusCode.OK)
            {
                var c = r.Content.ToModel<IPInfo>();
                if (c == default(IPInfo)) return new IPInfo()
                {
                    IsValid = false
                };
                c.IsValid = !bool.TryParse(c.Hostname, out bool re);
                return c;
            } else
            {
                return new IPInfo()
                {
                    IsValid = false
                };
            }
        }
    }
}
