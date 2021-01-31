using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Spatial;

namespace WBoostServer
{
    public class Donor
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("admit_date")]
        public string AdmitDate { get; set; }

        [JsonPropertyName("dob")]
        public string Dob { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("hospital")]
        public string Hospital { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("problem")]
        public string Problem { get; set; }

        [JsonPropertyName("patient_address")]
        public string PatientAddress { get; set; }

        [JsonPropertyName("Gender")]
        public string Gender { get; set; }

        [JsonPropertyName("latlong")]
        public Point LatLong { get; set; }

    }
}
