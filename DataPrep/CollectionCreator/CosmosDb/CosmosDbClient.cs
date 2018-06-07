using System;
using System.Net;
using System.Threading.Tasks;
using CollectionCreator.Config;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CollectionCreator.CosmosDb
{

    public class CosmosDbClient
    {
        
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
            Console.WriteLine("Removing small collection");
            await this.documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.SmallCollectionId));
        }
        public async Task RemoveLargeCollectionAsync()
        {
            Console.WriteLine("Removing large collection");
            await this.documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId));
        }

        public async Task CreateSmallCollectionIfNotExistsAsync()
        {
            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.SmallCollectionId, this.cosmosConfig.SmallDocumentCount, true);
        }

        public async Task CreateSampleDocuments(string databaseId, string collectionId, int count, bool useGoodPartitionKey)
        {
            Console.WriteLine("Creating {0} records", count);
            var sampleData = SampleDocumentCreator.Generate(count, useGoodPartitionKey);
            foreach (var d in sampleData)
            {
                var uri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                await CreateDocumentInCollectionAsync<DataDocument>(uri, d);
            };
        }
        public async Task CreateLargeCollectionIfNotExistsAsync()
        {
            await CreateDocumentCollectionIfNotExistsAsync(this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeThroughput);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeDocumentCount,true);
            await CreateSampleDocuments(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId, this.cosmosConfig.LargeDocumentCount, false);
        }
        private async Task CreateDocumentCollectionIfNotExistsAsync(string collectionId, int throughput)
        {
            await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = this.cosmosConfig.DatabaseId });
            //var hostedInCloud = !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
            // Note: We test for hosting in cloud so that if being run locally, no partition key and offer throughput
            //       is used in collection creation. While this works locally, the CosmosDb emulator Data explorer does not
            //       display any documents if using a partition key and actually errors making it look like there is no data
            //       when in fact there is.
            //if (hostedInCloud)
            //{
            var partitionKeyPaths = new PartitionKeyDefinition();
            partitionKeyPaths.Paths.Add("/partitionKey");
            await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(this.cosmosConfig.DatabaseId),
                    new DocumentCollection { Id = collectionId, PartitionKey = partitionKeyPaths },
                    new RequestOptions { OfferThroughput = throughput, ConsistencyLevel = ConsistencyLevel.Eventual });
            //}
            //else
            //{
            //    await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(
            //        UriFactory.CreateDatabaseUri(dataLocation.DatabaseId),
            //        new DocumentCollection { Id = dataLocation.CollectionId });
            //}

        }
    }
}