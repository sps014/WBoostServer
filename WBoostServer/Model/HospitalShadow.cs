using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WBoostServer
{
    public class HospitalShadow
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("number")]
        public string PhoneNumber { get; set; }
        [JsonPropertyName("donor")]
        public string Donor { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("bg")]
        public string BloodGroup { get; set; }
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        [JsonPropertyName("long")]
        public double Long { get; set; }
        [JsonPropertyName("url")]
        public string URL { get; set; }
    }
}
