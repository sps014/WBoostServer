using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Spatial;

namespace WBoostServer.Controllers
{
    [ApiController]
    [Route("Donor")]
    public class DonorController : ControllerBase
    {
        DocumentClient Cosmos;

        private const string DatabaseName = "loc";
        private const string CollectionName = "Donar";

        private Database DB;
        private Uri documentCollectionUri;

        private static readonly IndexingPolicy IndexingPolicyWithSpatialEnabledOnPoints = new IndexingPolicy(new SpatialIndex(DataType.Point), new RangeIndex(DataType.String) { Precision = -1 });

        private DocumentCollection Collection;
        public DonorController(DocumentClient Cosmos)
        {
            this.Cosmos = Cosmos;
            DB = GetDatabase().GetAwaiter().GetResult();
            Collection = GetCollection().GetAwaiter().GetResult(); ;
            documentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);

        }
        [HttpPost("Add")]
        public async Task<IActionResult> Add()
        {
            //ReadJson();
            //return Ok("Written Json to azure");

            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            Donor newHospital;
            try
            {
                var shadow = JsonSerializer.Deserialize<DonorShadow>(json);
                newHospital = new Donor()
                {
                    
                    LatLong = new Point(shadow.Long, shadow.Lat),
                    Address=shadow.Address,
                    AdmitDate=shadow.AdmitDate,
                    Dob=shadow.Dob,
                    Gender=shadow.Gender,
                    Hospital=shadow.Hospital,
                    Name=shadow.Name,
                    Number=shadow.Number,
                    PatientAddress=shadow.PatientAddress,
                    Problem=shadow.Problem
                  
                };
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            await Cosmos.CreateDocumentAsync(documentCollectionUri, newHospital);

            return Ok("success");
        }

        private async void ReadJson()
        {
            var json = await new StreamReader(@"C:\Users\shive\Downloads\data1.json").ReadToEndAsync();
            Donor newHospital;
            try
            {
                var shadows = JsonSerializer.Deserialize<DonorShadow[]>(json);
                foreach (var shadow in shadows)
                {
                    newHospital = new Donor()
                    {
                        LatLong = new Point(shadow.Long, shadow.Lat),
                        Address = shadow.Address,
                        AdmitDate = shadow.AdmitDate,
                        Dob = shadow.Dob,
                        Gender = shadow.Gender,
                        Hospital = shadow.Hospital,
                        Name = shadow.Name,
                        Number = shadow.Number,
                        PatientAddress = shadow.PatientAddress,
                        Problem = shadow.Problem
                    };

                    await Cosmos.CreateDocumentAsync(documentCollectionUri, newHospital);

                }
            }
            catch (Exception e)
            {

            }


            // return Ok("success");
        }
        [HttpPost("Locate")]
        public async Task<IActionResult> Locate()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            Donor newHospital;
            try
            {
                var shadow = JsonSerializer.Deserialize<LocateParam>(json);
                newHospital = new Donor()
                {
                    LatLong = new Point(double.Parse(shadow.Long), double.Parse(shadow.Lat))
                };
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            Point cpt = newHospital.LatLong;


            var maximumDistance = 150_000;//150km

            var qry = "SELECT * FROM loc e WHERE  ST_DISTANCE(e.LatLong, {'type': 'Point', 'coordinates':[" + cpt.Position.Longitude + "," + cpt.Position.Latitude + "]}) < " + $"{maximumDistance}";

            var nearQuery = Cosmos.CreateDocumentQuery<Donor>(documentCollectionUri,
                qry, new FeedOptions { EnableCrossPartitionQuery = true });

            //var nearQuery = Cosmos.CreateDocumentQuery<Hospital>(documentCollectionUri
            //    , new FeedOptions { EnableCrossPartitionQuery = true })
            //.Where(c => currentPt.Distance(c.LatLong) < maximumDistance);

            var nb = nearQuery.ToList();

            var b = nb.OrderBy((x) => Distance(cpt.Position.Latitude, cpt.Position.Longitude,
                  x.LatLong.Position.Latitude, x.LatLong.Position.Longitude, 'K')).ToList();

            return Ok(b);
        }

        async Task<Database> GetDatabase()
        {
            var dbs = from db in Cosmos.CreateDatabaseQuery()
                      where db.Id == DatabaseName
                      select db;

            var dbr = dbs.ToList().First();
            if (dbr == null)
                dbr = await Cosmos.CreateDatabaseAsync(new Database() { Id = DatabaseName });

            return dbr;
        }
        async Task<DocumentCollection> GetCollection()
        {
            var dbs = from col in Cosmos.CreateDocumentCollectionQuery(DB.SelfLink)
                      where col.Id == CollectionName
                      select col;

            var col1st = dbs.ToList().First();
            if (col1st == null)
                col1st = await Cosmos.CreateDocumentCollectionAsync(DB.SelfLink,
                    new DocumentCollection() { IndexingPolicy = IndexingPolicyWithSpatialEnabledOnPoints });

            return dbs.ToList().First();
        }
        private double Distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}
