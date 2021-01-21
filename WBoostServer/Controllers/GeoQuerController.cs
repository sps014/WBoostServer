using System;
using System.Collections.Generic;
using System.Linq;
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
            DB = GetDatabase();
            Collection = GetCollection();
            documentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);

        }
        [HttpGet("Add")]
        public IActionResult Add()
        {
            Cosmos.CreateDocumentAsync(
                documentCollectionUri
                ,new Hospital() { LatLong = new Point(13, 35), Address = "Barbodas" }
                ); 

            return Ok("query result");
        }
        [HttpGet("Locate")]
        public IActionResult Locate()
        {
            Point currentPt = new Point(12, 22);
            int maximumDistance = 120000;

            var nearQuery = Cosmos.CreateDocumentQuery<Hospital>(documentCollectionUri
                ,new FeedOptions { EnableCrossPartitionQuery = true })
            .Where(cda => currentPt.Distance(cda.LatLong) < maximumDistance)
            .AsDocumentQuery();

            var res = nearQuery.ExecuteNextAsync().GetAwaiter().GetResult();
            var nb=res.ToList().First();

            return Ok("query result");
        }

        Database GetDatabase()
        {
            var dbs = from db in Cosmos.CreateDatabaseQuery()
                      where db.Id == DatabaseName
                      select db;

            var dbr= dbs.ToList().First();
            if(dbr==null)
                dbr = Cosmos.CreateDatabaseAsync(new Database() { Id = DatabaseName })
                    .GetAwaiter()
                    .GetResult();

            return dbr;
        }
        DocumentCollection GetCollection()
        {
            var dbs = from col in Cosmos.CreateDocumentCollectionQuery(DB.SelfLink)
                      where col.Id==CollectionName
                      select col;

            var col1st = dbs.ToList().First();
            if (col1st == null)
                col1st = Cosmos.CreateDocumentCollectionAsync(DB.SelfLink,
                    new DocumentCollection() { IndexingPolicy = IndexingPolicyWithSpatialEnabledOnPoints })
                    .GetAwaiter()
                    .GetResult();

            return dbs.ToList().First();
        }

    }
}
