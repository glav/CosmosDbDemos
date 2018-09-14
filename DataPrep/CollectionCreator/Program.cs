using CollectionCreator.Config;
using CosmosCommon.Config;
using CosmosCommon.CosmosDb;
using CosmosCommon.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CollectionCreator
{
    class Program
    {
        public static DbCollectionCreatorClient _client;

        static void Main(string[] args)
        {
            var cosmosConfig = AppConfig.GetCosmosConfig();
            _client = new DbCollectionCreatorClient(cosmosConfig);
            if (string.IsNullOrWhiteSpace(cosmosConfig.KeySql) || string.IsNullOrWhiteSpace(cosmosConfig.KeyGraph))
            {
                Console.WriteLine("\nWARNING!: No access key defined. This action will probably fail.\n");
            }

            var app = CommandLineInitialiser.SetupCommandLineOptions(_client);

            app.OnExecute(() =>
            {
                Logger.Write("\nCosmosDb Demo Collection Creator");

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
                        Logger.Write($"Warning: [{docex.StatusCode}] - Ignoring");
                        return true;
                    }
                    return false;
                });
            }
        }


    }
}
