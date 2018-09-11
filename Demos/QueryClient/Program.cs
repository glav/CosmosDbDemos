using CosmosCommon.Config;
using System;

namespace QueryClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var cosmosConfig = AppConfig.GetCosmosConfig();
            int iterationCount;

            if (args.Length == 0 || !int.TryParse(args[0], out iterationCount))
            {
                iterationCount = 1;
            }

            Console.WriteLine($"Running query {iterationCount} times...");

            var client = new QueryRunner(cosmosConfig);

            //var results = client.SimpleQueryResultSetsAsync().Result;
            var results = client.ContinuousQuery().Result;

        }
    }
}