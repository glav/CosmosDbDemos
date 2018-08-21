using CosmosCommon.CosmosDb;
using CosmosCommon.Helpers;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator.Config
{
    internal static class CommandLineInitialiser
    {
        public static CommandLineApplication SetupCommandLineOptions(DbCollectionCreatorClient client)
        {
            var app = new CommandLineApplication();
            app.Name = "CosmosDb Demo Collection Creator";
            app.Description = "Simple app to create demo collections and also sample data";
            app.HelpOption("--? | --help | -h | -?");

            app.Command("createall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Logger.Write("Creating all collections");
                    client.CreateSmallCollectionIfNotExistsAsync().Wait();
                    client.CreateLargeCollectionIfNotExistsAsync().Wait();
                    Logger.Write("Collections created");
                    return 0;
                });
            });

            app.Command("createsmall", (command) =>
            {
                command.OnExecute(() =>
                {
                    client.CreateSmallCollectionIfNotExistsAsync().Wait();
                    return 0;
                });
            });

            app.Command("createlarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    client.CreateLargeCollectionIfNotExistsAsync().Wait();
                    return 0;
                });
            });

            app.Command("deletesmall", (command) =>
            {
                command.OnExecute(() =>
                {
                    client.RemoveSmallCollectionAsync().Wait();
                    return 0;
                });
            });

            app.Command("deletelarge", (command) =>
            {
                command.OnExecute(() =>
                {
                    client.RemoveLargeCollectionAsync().Wait();
                    return 0;
                });
            });

            app.Command("deleteall", (command) =>
            {
                command.OnExecute(() =>
                {
                    Logger.Write("Deleting all collections");
                    client.RemoveSmallCollectionAsync().Wait();
                    client.RemoveLargeCollectionAsync().Wait();
                    Logger.Write("Collections deleted");
                    return 0;
                });
            });

            app.Command("run1query",(command) => 
            {
                command.OnExecute(() =>
                {
                    var total = client.RunQueries(1);
                    return 0;
                });
            });
            app.Command("run10queries", (command) =>
            {
                command.OnExecute(() =>
                {
                    var total = client.RunQueries(10);
                    return 0;
                });
            });
            app.Command("run100queries", (command) =>
            {
                command.OnExecute(() =>
                {
                    var total = client.RunQueries(10);
                    return 0;
                });
            });

            return app;
        }
    }
}
