using CosmosCommon.CosmosDb;
using CosmosCommon.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollectionCreator
{

    public class DbCollectionCreatorClient
    {
        private readonly DocumentClient documentSqlClient;
        private readonly DocumentClient documentGraphClient;
        private readonly CosmosDbConfig cosmosConfig;

        public DbCollectionCreatorClient(CosmosDbConfig cosmosConfig)
        {
            this.cosmosConfig = cosmosConfig;
            this.documentSqlClient = new DocumentClient(new Uri(cosmosConfig.CosmosDbSqlEndpoint), cosmosConfig.KeySql,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp, MaxConnectionLimit = 1000 });
            this.documentGraphClient = new DocumentClient(new Uri(cosmosConfig.CosmosDbGraphEndpoint), cosmosConfig.KeyGraph,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp, MaxConnectionLimit = 1000 });
            this.documentSqlClient.OpenAsync();
            this.documentGraphClient.OpenAsync();
        }

        public async Task CreateDocumentInCollectionAsync<T>(Uri uri, T document) where T : class
        {
            await this.documentSqlClient.UpsertDocumentAsync(uri, document);
        }

        public async Task RemoveSmallCollectionAsync()
        {
            Logger.Write("Removing small collection");
            await this.documentSqlClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.SmallCollectionId));
            await this.documentGraphClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseIdGRAPH, this.cosmosConfig.SmallCollectionId));
        }
        public async Task RemoveLargeCollectionAsync()
        {
            Logger.Write("Removing large collection");
            await this.documentSqlClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.LargeCollectionId));
        }

        public async Task CreateSmallCollectionIfNotExistsAsync()
        {
            Logger.Write($"Creating small document collection [{this.cosmosConfig.SmallCollectionId}]");
            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallThroughput);
            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.DatabaseIdGRAPH, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallDocumentCount, true);
            //await CreateSampleDocuments(this.cosmosConfig.DatabaseIdGRAPH, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallDocumentCount, true);
        }

        public int GetRecordCount(string collectionId)
        {
            Logger.Write("Checking for existing records...");
            var collectionLink = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseIdSQL, collectionId);
            var query = this.documentSqlClient.CreateDocumentQuery(collectionLink, "SELECT VALUE COUNT(1) FROM c").ToList();
            var cnt = (int)query.First();
            Logger.Write($"Current record count: {cnt}");
            return cnt;

        }

        public async Task CreateSampleDocuments(string databaseId, string collectionId, int count, bool useGoodPartitionKey)
        {
            Logger.Write($"Creating {count} records");
            var recCnt = GetRecordCount(collectionId);

            var sampleData = SampleDocumentCreator.Generate(count, useGoodPartitionKey, recCnt);
            foreach (var d in sampleData)
            {
                var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                await CreateDocumentInCollectionAsync<DataDocument>(uri, d);
            };
        }
        public async Task CreateLargeCollectionIfNotExistsAsync()
        {
            Logger.Write($"Creating large document collection [{this.cosmosConfig.LargeCollectionId}]");

            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.LargeCollectionId, ((int)this.cosmosConfig.LargeDocumentCount / 2), true);
            try
            {
                await CreateSampleDocuments(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.LargeCollectionId, ((int)this.cosmosConfig.LargeDocumentCount / 2), false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Problem creating records for static partition");
            }
            Console.WriteLine("Running some queries to generate metrics");
            await RunQueriesAsync(100);
        }

        public async Task<int> RunQueriesAsync(int count)
        {
            int total = 0;
            var rnd = new Random(DateTime.Now.Millisecond);
            Logger.Write($"Running query {count} times.");
            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseIdSQL, this.cosmosConfig.LargeCollectionId);
            for (var loop = 0; loop < count; loop++)
            {

                await Task.Run(() =>
                {
                    //sometimes we want to query using the 'bad' or static partition and other times we don't just to generate different looking metrics
                    var useStaticKey = rnd.Next(0, 10) >= 7;

                    List<dynamic> query;
                    Console.Write(".");
                    if (useStaticKey)
                    {
                        query = this.documentSqlClient.CreateDocumentQuery(uri, "SELECT c.id,c.partitionKey FROM c where contains (c.id,\"12345\") ",
                            new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 }).ToList();
                    }
                    else
                    {
                        var searchText = rnd.Next(1, 10);
                        query = this.documentSqlClient.CreateDocumentQuery(uri, $"SELECT c.id,c.partitionKey FROM c where contains (c.id,\"{searchText}\") ",
                            new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1 }).ToList();
                    }
                    total += query.Count;

                });
            }

            Logger.Write("Queries complete");
            return total;
        }
        private async Task CreateDocumentCollectionIfNotExistsAsync(string databaseId, string collectionId, int throughput)
        {
            var client = databaseId == this.cosmosConfig.DatabaseIdSQL ? this.documentSqlClient : this.documentGraphClient;
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            var partitionKeyPaths = new PartitionKeyDefinition();
            partitionKeyPaths.Paths.Add("/partitionKey");
            await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseId),
                    new DocumentCollection { Id = collectionId, PartitionKey = partitionKeyPaths },
                    new RequestOptions { OfferThroughput = throughput, ConsistencyLevel = ConsistencyLevel.Eventual });
        }
    }
}
