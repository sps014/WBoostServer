using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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

        private static readonly IndexingPolicy IndexingPolicyWithSpatialEnabledOnPoints = new IndexingPolicy(new SpatialIndex(DataType.Point), new RangeIndex(DataType.String) { Precision = -1 });

        private DocumentCollection Collection;
        public GeoQuery(DocumentClient Cosmos)
        {
            this.Cosmos = Cosmos;
            DB = GetDatabase();
            Collection = GetCollection();

        }
        [HttpGet("Add")]
        public IActionResult Add()
        {
            Cosmos.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName), new Hospital()
                { LatLong = new Point(12, 22), Address = "Barbodas" }
                ); ;

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
