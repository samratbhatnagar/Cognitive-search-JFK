using CognitiveSearchInitializer.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CognitiveSearchInitializer
{
    internal class Program
    {
        private static IConfigurationRoot _configuration;

        static void Main(string[] args)
        {
            // Setup configuration to read from the appsettings.json file.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();
            var appConfig = new AppConfig();
            _configuration.Bind(appConfig);

            using (var searchClient = new SearchServiceClient(appConfig.Search.ServiceName, new SearchCredentials(appConfig.Search.Key)))
            {
            }
        }
    }
}
