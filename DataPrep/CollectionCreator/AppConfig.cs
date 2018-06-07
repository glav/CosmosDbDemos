using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator
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
    }
}
