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
        private int spinnerPos = 0;
        private string[] spinnerChars = new string[] { "|", "/", "-", "\\", "|", "/", "-", "\\" };
        private Random rnd = new Random(DateTime.Now.Millisecond);

        #region Setup client in ctor
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
        #endregion

        public async Task<List<DataDocument>> SimpleQueryResultSetsAsync()
        {

            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);
            var results = new List<DataDocument>();

            using (var query = this.documentClient.CreateDocumentQuery(uri,
                "SELECT c.id,c.partitionKey,c.address.state FROM c where contains (c.appId,\"1\") ",
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = true,
                        //MaxDegreeOfParallelism = -1
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

        public async Task<List<DataDocument>> ContinuousQuery()
        {

            var uri = UriFactory.CreateDocumentCollectionUri(this.cosmosConfig.DatabaseId, this.cosmosConfig.LargeCollectionId);
            var results = new List<DataDocument>();
            Console.WriteLine("Performing queries:");

            while (true)
            {
                var queryText = rnd.Next(1, 9).ToString();
                using (var query = this.documentClient.CreateDocumentQuery(uri,
                    "SELECT top 50 c.id,c.partitionKey,c.useGoodPartitionKey " +
                        $"FROM c where contains (c.appId,'{queryText}') and c.useGoodPartitionKey=true",
                        new FeedOptions { EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1
                    }).AsDocumentQuery())
                {

                    while (query.HasMoreResults)
                    {
                        var nextResults = await query.ExecuteNextAsync<DataDocument>();
                        results.AddRange(nextResults);

                    }
                    #region Console output
                    SpinProgressMeter(results.Count);
                    if (Console.KeyAvailable)
                    {
                        break;
                    }
                    #endregion

                }
            }
            return results.ToList();
        }

        private void SpinProgressMeter(int resultCount)
        {
            if (resultCount % 200 != 0)
                return;

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(spinnerChars[spinnerPos]);
            spinnerPos++;
            if (spinnerPos > spinnerChars.Length-1) { spinnerPos = 0; }
        }
    }
}
