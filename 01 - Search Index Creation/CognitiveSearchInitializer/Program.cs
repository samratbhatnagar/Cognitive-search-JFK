using CognitiveSearchInitializer.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CognitiveSearchInitializer
{
    internal class Program
    {
        private static IConfigurationRoot _configuration;

        static async Task Main(string[] args)
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
                var dataSource = await CognitiveSearchHelper.GetOrCreateBlobDataSource(searchClient, appConfig.Search.DataSourceName, DataSourceType.AzureBlob, appConfig.BlobStorage);
                var baseIndex = await CognitiveSearchHelper.GetIndexFromFile("index-base");
                var baseSkillset = await CognitiveSearchHelper.GetSkillsetFromFile("skillset-base", appConfig);
                var baseIndexer = await CognitiveSearchHelper.GetIndexerFromFile(appConfig.Search);
                await CognitiveSearchHelper.CreateCognitiveSearchPipeline(searchClient, appConfig.Search, baseIndex, baseIndexer, baseSkillset)
                    .ContinueWith(t =>
                    {
                        Console.WriteLine(t.IsFaulted ? t.Exception.Message : "Your pipeline was successfully created.");
                    });

            }
        }
    }
}
