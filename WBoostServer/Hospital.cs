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
        public string Title { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
        public string Donar { get; set; }
        public string Date { get; set; }
        [JsonProperty("bg")]
        public string BloodGroup { get; set; }
        [JsonProperty("latlong")]
        public Point LatLong { get; set; }
    }
}
