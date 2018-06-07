using CollectionCreator.Config;
using CollectionCreator.CosmosDb;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CollectionCreator
{
    class Program
    {
        public static CosmosDbClient _client;

        static void Main(string[] args)
        {
            var cosmosConfig = GetCosmosConfig();
            _client = new CosmosDbClient(cosmosConfig);

            var app = CommandLineInitialiser.SetupCommandLineOptions(_client);

            app.OnExecute(() =>
            {
                Console.WriteLine("\nCosmosDb Demo Collection Creator");

                return 0;
            });
            try
            {
                app.Execute(args);
            } catch (AggregateException aex)
            {
                aex.Handle((x) => {
                    var docex = x as DocumentClientException;
                    if (docex != null && docex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("Warning: [{0}] - Ignoring", docex.StatusCode);
                        return true;
                    }
                    return false;
                });
            }
        }

        private static CosmosDbConfig GetCosmosConfig()
        {

            return new CosmosDbConfig(
                AppConfig.DatabaseId)
            {
                SmallCollectionId = AppConfig.SmallCollectionId,
                LargeCollectionId = AppConfig.LargeCollectionId,
                Endpoint = AppConfig.CosmosDbEndpoint,
                Key = AppConfig.CosmosDbKey,
                SmallThroughput = AppConfig.SmallThroughput,
                LargeThroughput = AppConfig.LargeThroughput
            };
        }

    }
}
