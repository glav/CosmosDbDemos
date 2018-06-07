using CollectionCreator.CosmosDb;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator.Config
{
    internal static class CommandLineInitialiser
    {
        public static CommandLineApplication SetupCommandLineOptions(CosmosDbClient client)
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
                    client.CreateSmallCollectionIfNotExistsAsync().Wait();
                    Console.WriteLine("Collection created");
                    return 0;
                });
            });

            app.Command("createlarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Creating large collection");
                    client.CreateLargeCollectionIfNotExistsAsync().Wait();
                    Console.WriteLine("Collection created");
                    return 0;
                });
            });

            app.Command("deletesmall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Deleting small collection");
                    client.RemoveSmallCollectionAsync().Wait();
                    Console.WriteLine("Collection deleted");
                    return 0;
                });
            });

            app.Command("deletelarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Deleting large collection");
                    client.RemoveLargeCollectionAsync().Wait();
                    Console.WriteLine("Collection deleted");
                    return 0;
                });
            });

            app.Command("deleteall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Console.WriteLine("Deleting all collections");
                    client.RemoveSmallCollectionAsync().Wait();
                    client.RemoveLargeCollectionAsync().Wait();
                    Console.WriteLine("Collections deleted");
                    return 0;
                });
            });

            return app;
        }
    }
}
