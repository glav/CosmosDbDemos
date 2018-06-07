using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator.Config
{
    public static class AppConfig
    {
        public static IConfigurationRoot Values { get; private set; }
        static AppConfig()
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("AppConfig.json");
            if (Environment.GetEnvironmentVariable("BuildConfiguration") == "development")
            {
                configBuilder.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly());
            }
            Values = configBuilder.Build();
        }

        public static string DatabaseId { get { return Values["DatabaseId"]; } }
        public static string SmallCollectionId { get { return Values["SmallCollectionId"]; } }
        public static string LargeCollectionId { get { return Values["LargeCollectionId"]; } }
        public static string CosmosDbEndpoint { get { return Values["CosmosDbEndpoint"]; } }
        public static string CosmosDbKey { get { return Values["CosmosDbKey"]; } }
        public static int SmallThroughput { get { return string.IsNullOrWhiteSpace(Values["SmallThroughput"]) ? 400 : Convert.ToInt32(Values["SmallThroughput"]); } }
        public static int LargeThroughput { get { return string.IsNullOrWhiteSpace(Values["LargeThroughput"]) ? 12000 : Convert.ToInt32(Values["LargeThroughput"]); } }

    }
}
