using CosmosCommon.CosmosDb;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosCommon.Config
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

        public static string DatabaseIdPrefix { get { return Values["DatabaseIdPrefix"]; } }
        public static string SmallCollectionId { get { return Values["SmallCollectionId"]; } }
        public static string LargeCollectionId { get { return Values["LargeCollectionId"]; } }
        public static string CosmosDbSqlEndpoint { get { return Values["CosmosDbSqlEndpoint"]; } }
        public static string CosmosDbGraphEndpoint { get { return Values["CosmosDbGraphEndpoint"]; } }
        public static string CosmosDbKey { get { return Values["CosmosDbKey"]; } }
        public static string CosmosDbSqlKey { get { return Values["CosmosDbSqlKey"]; } }
        public static string CosmosDbGraphKey { get { return Values["CosmosDbGraphKey"]; } }
        public static int SmallThroughput { get { return GetSafeInt("SmallThroughput", 400); } }
        public static int LargeThroughput { get { return GetSafeInt("LargeThroughput",12000); } }
        public static int SmallDocumentCount { get { return GetSafeInt("SmallDocumentCount", 3); } }
        public static int LargeDocumentCount { get { return GetSafeInt("LargeDocumentCount", 1000); } }

        private static int GetSafeInt(string key, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(Values[key]))
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToInt32(Values[key]);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static CosmosDbConfig GetCosmosConfig()
        {

            return new CosmosDbConfig(
                AppConfig.DatabaseIdPrefix)
            {
                SmallCollectionId = AppConfig.SmallCollectionId,
                LargeCollectionId = AppConfig.LargeCollectionId,
                CosmosDbSqlEndpoint = AppConfig.CosmosDbSqlEndpoint,
                CosmosDbGraphEndpoint = AppConfig.CosmosDbGraphEndpoint,
                KeySql = AppConfig.CosmosDbSqlKey,
                KeyGraph = AppConfig.CosmosDbGraphKey,
                SmallThroughput = AppConfig.SmallThroughput,
                LargeThroughput = AppConfig.LargeThroughput,
                SmallDocumentCount = AppConfig.SmallDocumentCount,
                LargeDocumentCount = AppConfig.LargeDocumentCount
            };
        }

    }
}
