using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace WBoostServer
{
    public record Hospital
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("number")]
        public string PhoneNumber { get; set; }
        [JsonProperty("donor")]
        public string Donar { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("bg")]
        public string BloodGroup { get; set; }
        [JsonProperty("latlong")]
        public Point LatLong { get; set; }
    }
}
