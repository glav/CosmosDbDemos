using System;
using System.Net;
using System.Threading.Tasks;
using CollectionCreator.Config;
using CollectionCreator.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;

namespace CollectionCreator.CosmosDb
{

    public class CosmosDbClient
    {
        private readonly AsyncRunner asynRunner = new AsyncRunner();
        private readonly DocumentClient documentClient;
        private readonly CosmosDbConfig cosmosConfig;

        public CosmosDbClient(CosmosDbConfig cosmosConfig)
        {
            this.cosmosConfig = cosmosConfig;
            this.documentClient = new DocumentClient(new Uri(cosmosConfig.Endpoint), cosmosConfig.Key,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp, MaxConnectionLimit = 1000 });
            this.documentClient.OpenAsync();
        }

        public async Task CreateDocumentInCollectionAsync<T>(Uri uri, T document) where T : class
        {
            await this.documentClient.UpsertDocumentAsync(uri, document);
        }

        public async Task RemoveSmallCollectionAsync()
        {
            Logger.Write("Removing small collection");
            await this.documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.SmallCollectionId));
        }
        public async Task RemoveLargeCollectionAsync()
        {
            Logger.Write("Removing large collection");
            await this.documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId));
        }

        public async Task CreateSmallCollectionIfNotExistsAsync()
        {
            Logger.Write($"Creating small document collection [{this.cosmosConfig.SmallCollectionId}]");
            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallDocumentCount, true);
        }

        public int GetRecordCount(string collectionId)
        {
            Logger.Write("Checking for existing records...");
            var collectionLink = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, collectionId);
            var query = this.documentClient.CreateDocumentQuery(collectionLink, "SELECT VALUE COUNT(1) FROM c").ToList();
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

                asynRunner.AddTaskToRunConcurrently(CreateDocumentInCollectionAsync<DataDocument>(uri, d));
                //await CreateDocumentInCollectionAsync<DataDocument>(uri, d);
            };
            await asynRunner.RunAllTasks();
        }
        public async Task CreateLargeCollectionIfNotExistsAsync()
        {
            Logger.Write($"Creating large document collection [{this.cosmosConfig.LargeCollectionId}]");

            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeDocumentCount, true);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeDocumentCount, false);
        }

        public int RunQueries(int count)
        {
            int total = 0;
            Logger.Write($"Running query {count} times.");
            this.asynRunner.ClearTasks();
            for (var loop = 0; loop < count; loop++)
            {

                var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);

                this.asynRunner.AddTaskToRunConcurrently(Task.Run(() =>
                {

                    var query = this.documentClient.CreateDocumentQuery(uri, "SELECT c.id,c.partitionKey FROM c where contains (c.id,\"12345\") ", new FeedOptions { EnableCrossPartitionQuery = true }).ToList();
                    total += query.Count;
                }));
           }
            this.asynRunner.RunAllTasks().Wait();
            return total;
        }
        private async Task CreateDocumentCollectionIfNotExistsAsync(string collectionId, int throughput)
        {
            await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = this.cosmosConfig.DatabaseId });

            var partitionKeyPaths = new PartitionKeyDefinition();
            partitionKeyPaths.Paths.Add("/partitionKey");
            await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(this.cosmosConfig.DatabaseId),
                    new DocumentCollection { Id = collectionId, PartitionKey = partitionKeyPaths },
                    new RequestOptions { OfferThroughput = throughput, ConsistencyLevel = ConsistencyLevel.Eventual });
        }
    }
}