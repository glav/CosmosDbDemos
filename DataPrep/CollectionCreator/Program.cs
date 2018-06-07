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
            var app = SetupCommandLineOptions();
            var cosmosConfig = GetCosmosConfig(app);
            _client = new CosmosDbClient(cosmosConfig);

            app.OnExecute(() =>
            {
                Console.WriteLine("\nCosmosDb Demo Collection Creator");

                return 0;
            });
            app.Execute(args);
        }

        private static CosmosDbConfig GetCosmosConfig(CommandLineApplication app)
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

        private static CommandLineApplication SetupCommandLineOptions()
        {
            var app = new CommandLineApplication();
            app.Name = "CosmosDb Demo Collection Creator";
            app.Description = "Simple app to create demo collections and also sample data";
            app.HelpOption("--? | --help | -h | -?");

            app.Command("createsmall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Creating small collection");
                    _client.CreateSmallCollectionIfNotExistsAsync().Wait();
                    Console.WriteLine("Collection created");
                    return 0;
                });
            });

            app.Command("createlarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Creating large collection");
                    _client.CreateLargeCollectionIfNotExistsAsync().Wait();
                    Console.WriteLine("Collection created");
                    return 0;
                });
            });

            app.Command("deletesmall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Deleting small collection");
                    _client.RemoveSmallCollectionAsync().Wait();
                    Console.WriteLine("Collection deleted");
                    return 0;
                });
            });

            app.Command("deletelarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Deleting large collection");
                    _client.RemoveLargeCollectionAsync().Wait();
                    Console.WriteLine("Collection deleted");
                    return 0;
                });
            });

            var action = app.Option("--t", "Throughput RU's to be used/applied", CommandOptionType.SingleValue);
            action.ShortName = AppOptions.Throughput;

            return app;
        }
    }
}
