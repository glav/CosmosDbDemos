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

            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            connectionPolicy.PreferredLocations.Add(LocationNames.AustraliaEast);
            connectionPolicy.PreferredLocations.Add(LocationNames.AustraliaSoutheast);

            this.documentClient = new DocumentClient(new Uri(cosmosConfig.Endpoint), cosmosConfig.Key,connectionPolicy);
            this.documentClient.OpenAsync();
        }

        public async Task<List<DataDocument>> SimpleQueryResultSetsAsync()
        {

            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);
            var results = new List<DataDocument>();

            using (var query = this.documentClient.CreateDocumentQuery(uri,
                "SELECT c.id,c.partitionKey,c.address.state FROM c where contains (c.appId,\"1\") ",
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true,
                        //MaxDegreeOfParallelism = 1
                    }).AsDocumentQuery())
            {

                #region Uninteresting bits
                var watch = new System.Diagnostics.Stopwatch();
                var consoleBuffer = new StringBuilder();
                consoleBuffer.Append("\nRetrieving records in batches\n\t");
                watch.Start();
                #endregion

                while (query.HasMoreResults)
                {
                    var nextResults = await query.ExecuteNextAsync<DataDocument>();
                    results.AddRange(nextResults);

                    consoleBuffer.AppendFormat("\t{0} records\n", nextResults.Count);
                }

                #region More uninteresting bits
                watch.Stop();
                Console.WriteLine(consoleBuffer.ToString());
                Console.WriteLine("Finished querying in {0}:{1}.{2}", watch.Elapsed.Minutes, watch.Elapsed.Seconds, watch.Elapsed.Milliseconds);
                Console.WriteLine("Total records: {0}", results.Count);
                #endregion
            }
            return results.ToList();
        }

    }
}
