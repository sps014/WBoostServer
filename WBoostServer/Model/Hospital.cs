using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace WBoostServer
{
    public record Hospital
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("number")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("donor")]
        public string Donar { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("bg")]
        public string BloodGroup { get; set; }
        [JsonPropertyName("latlong")]
        public Point LatLong { get; set; }
        [JsonPropertyName("url")]
        public string URL { get; set; }
    }
}
