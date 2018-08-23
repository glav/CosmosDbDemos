using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using CosmosCommon.Helpers;
using CosmosCommon.CosmosDb;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;

namespace QueryClient
{

    public class QueryRunner
    {
        private readonly AsyncRunner asynRunner = new AsyncRunner();
        private readonly DocumentClient documentClient;
        private readonly CosmosDbConfig cosmosConfig;

        public QueryRunner(CosmosDbConfig cosmosConfig)
        {
            this.cosmosConfig = cosmosConfig;

            this.documentClient = new DocumentClient(new Uri(cosmosConfig.Endpoint), cosmosConfig.Key,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp, MaxConnectionLimit = 1000 });
            this.documentClient.OpenAsync();
        }

        public async Task<List<DataDocument>> SimpleQueryAsync()
        {

            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);
            var results = new List<DataDocument>();

            var query = this.documentClient.CreateDocumentQuery(uri,
                "SELECT c.id,c.partitionKey FROM c where contains (c.appId,\"12\") ",
                    new FeedOptions {
                                EnableCrossPartitionQuery = true,
                    //            MaxDegreeOfParallelism = -1
                    }).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                Console.WriteLine(" > Getting next set of results");

                var nextResults = await query.ExecuteNextAsync<DataDocument>();

                Console.WriteLine(" > Retrieved {0} results", nextResults.Count);
                results.AddRange(nextResults);
            }
            return results.ToList();
        }
    }
}
