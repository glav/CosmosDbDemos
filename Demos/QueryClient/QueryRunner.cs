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
using System.Timers;
using System.Text;

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
                new ConnectionPolicy {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp,
                        MaxConnectionLimit = 1000
                });
            this.documentClient.OpenAsync();
        }

        public async Task<List<DataDocument>> SimpleQueryAsync()
        {

            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);
            var results = new List<DataDocument>();

            using (var query = this.documentClient.CreateDocumentQuery(uri,
                "SELECT c.id,c.partitionKey FROM c where contains (c.appId,\"1\") ",
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true,
                        MaxDegreeOfParallelism = 1
                    }).AsDocumentQuery())
            {

                var watch = new System.Diagnostics.Stopwatch();
                var consoleBuffer = new StringBuilder();
                watch.Start();
                while (query.HasMoreResults)
                {
                    var nextResults = await query.ExecuteNextAsync<DataDocument>();

                    results.AddRange(nextResults);
                }
                watch.Stop();

                Console.WriteLine(consoleBuffer.ToString());
                Console.WriteLine("Finished querying in {0}:{1}.{2}", watch.Elapsed.Minutes, watch.Elapsed.Seconds, watch.Elapsed.Milliseconds);
            }
            return results.ToList();
        }
    }
}
