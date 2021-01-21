using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Spatial;

namespace WBoostServer.Controllers
{
    [Route("GeoQuery")]
    [ApiController]
    public class GeoQuery:ControllerBase
    {

        DocumentClient Cosmos;

        private const string DatabaseName="loc";
        private const string CollectionName= "Hospitals";

        private Database DB;
        private Uri documentCollectionUri;

        private static readonly IndexingPolicy IndexingPolicyWithSpatialEnabledOnPoints = new IndexingPolicy(new SpatialIndex(DataType.Point), new RangeIndex(DataType.String) { Precision = -1 });

        private DocumentCollection Collection;
        public GeoQuery(DocumentClient Cosmos)
        {
            this.Cosmos = Cosmos;
            DB = GetDatabase().GetAwaiter().GetResult();
            Collection = GetCollection().GetAwaiter().GetResult(); ;
            documentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);

        }
        [HttpPost("Add")]
        public async Task<IActionResult> Add()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            Hospital newHospital;
            try
            {
                var shadow = JsonSerializer.Deserialize<HospitalShadow>(json);
                newHospital=new Hospital() 
                { 
                    Address = shadow.Address,
                    BloodGroup=shadow.BloodGroup,
                    Date=shadow.Date,
                    Donar=shadow.Donor,
                    LatLong=new Point(shadow.Long,shadow.Lat),
                    PhoneNumber=shadow.PhoneNumber,
                    Title=shadow.Title,
                    URL=shadow.URL
                };
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

            await Cosmos.CreateDocumentAsync(documentCollectionUri, newHospital);

            return Ok("success");
        }
        [HttpPost("Locate")]
        public async Task<IActionResult> Locate()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            Hospital newHospital;
            try
            {
                var shadow = JsonSerializer.Deserialize<HospitalShadow>(json);
                newHospital = new Hospital()
                {
                    BloodGroup = shadow.BloodGroup,
                    LatLong = new Point(shadow.Long, shadow.Lat)                
                };
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            Point cpt =newHospital.LatLong;
            string bg = newHospital.BloodGroup;


            var maximumDistance = 150_000;//150km

            var qry = "SELECT * FROM loc e WHERE  ST_DISTANCE(e.latlong, {'type': 'Point', 'coordinates':[" + newHospital.LatLong.Position.Longitude + "," + newHospital.LatLong.Position.Latitude +"]}) < "+$"{maximumDistance}";
                 if(bg!=null)     qry+="  and e.bg="+'"'+newHospital.BloodGroup+'"';
            
            var nearQuery = Cosmos.CreateDocumentQuery<Hospital>(documentCollectionUri,
                qry, new FeedOptions { EnableCrossPartitionQuery = true });

            //var nearQuery = Cosmos.CreateDocumentQuery<Hospital>(documentCollectionUri
            //    , new FeedOptions { EnableCrossPartitionQuery = true })
            //.Where(c => currentPt.Distance(c.LatLong) < maximumDistance);

            var nb = nearQuery.ToList();
                nb.OrderBy((x)=>x.LatLong.Distance(cpt));

            return Ok(nb);
        }

        async Task<Database> GetDatabase()
        {
            var dbs = from db in Cosmos.CreateDatabaseQuery()
                      where db.Id == DatabaseName
                      select db;

            var dbr= dbs.ToList().First();
            if (dbr == null)
                dbr = await Cosmos.CreateDatabaseAsync(new Database() { Id = DatabaseName });

            return dbr;
        }
        async Task<DocumentCollection> GetCollection()
        {
            var dbs = from col in Cosmos.CreateDocumentCollectionQuery(DB.SelfLink)
                      where col.Id==CollectionName
                      select col;

            var col1st = dbs.ToList().First();
            if (col1st == null)
                col1st = await Cosmos.CreateDocumentCollectionAsync(DB.SelfLink,
                    new DocumentCollection() { IndexingPolicy = IndexingPolicyWithSpatialEnabledOnPoints });

            return dbs.ToList().First();
        }

    }
}
