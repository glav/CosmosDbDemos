using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CollectionCreator
{

    public class CosmosDbClient<T> where T : class
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

        public async Task CreateDocumentInCollectionAsync(Uri uri, T document)
        {
            await this.documentClient.UpsertDocumentAsync(uri, document);
        }

        public async Task CreateDocumentCollectionIfNotExistsAsync()
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
            partitionKeyPaths.Paths.Add("/PartitionKey");
            await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(this.cosmosConfig.DatabaseId),
                    new DocumentCollection { Id = this.cosmosConfig.CollectionId, PartitionKey = partitionKeyPaths },
                    new RequestOptions { OfferThroughput = this.cosmosConfig.Throughput, ConsistencyLevel = ConsistencyLevel.Eventual });
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