using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CollectionCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = SetupCommandLineOptions();
            var resourceGroup = app.ResourceGroupOption();

            if (app.Arguments.Count == 0)
            {
                app.ShowHelp();
                return;
            }


            app.OnExecute(() =>
            {
                Console.WriteLine("ResourceGroup , value: {0}", resourceGroup.Value());

                return 0;
            });
            app.Execute(args);
        }

        private static CommandLineApplication SetupCommandLineOptions()
        {
            var app = new CommandLineApplication();
            app.Name = "CosmosDb Demo Collection Creator";
            app.Description = "Simple app to create demo collections and also sample data";
            app.HelpOption("--? | --help | -h | -?");

            var action = app.Option("--action | -a", "Action to perform. Supported is 'demodata'", CommandOptionType.SingleValue);
            action.ShortName = AppOptions.Action;

            return app;
        }
    }
}
