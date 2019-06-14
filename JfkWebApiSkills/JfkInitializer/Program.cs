using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;

namespace JfkInitializer
{
    class Program
    {
        // Configurable names, these can be changed in the App.config file if you like
        private static string DataSourceName;
        private static string IndexName;
        private static string FileSkillsetName;
        private static string TextSkillsetName;

        private static IndexSource[] IndexSources;

        private static string IndexerPrefix;
        private static string SynonymMapName;
        private static string BlobContainerNameForImageStore;

        // Set this to true to see additional debugging information in the console.
        private static bool DebugMode = true;

        // Set this to true if you would like this app to deploy the JFK files frontend to your Azure site.
        private static bool ShouldDeployWebsite = true;

        // Clients
        private static ISearchServiceClient _searchClient;
        private static string _searchApiVersion = "2019-05-06";
        private static HttpClient _httpClient = new HttpClient();
        private static string _searchServiceEndpoint;
        private static string _azureFunctionHostKey;

        static void Main(string[] args)
        {
            DataSourceName = ConfigurationManager.AppSettings["DataSourceName"];
            IndexName = ConfigurationManager.AppSettings["IndexName"];
            FileSkillsetName = ConfigurationManager.AppSettings["FileSkillsetName"];
            TextSkillsetName = ConfigurationManager.AppSettings["TextSkillsetName"];
            IndexerPrefix = ConfigurationManager.AppSettings["IndexerPrefix"];
            SynonymMapName = ConfigurationManager.AppSettings["SynonymMapName"];
            BlobContainerNameForImageStore = ConfigurationManager.AppSettings["BlobContainerNameForImageStore"];

            string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            string apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

            _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            _searchServiceEndpoint = String.Format("https://{0}.{1}", searchServiceName, _searchClient.SearchDnsSuffix);

            using (StreamReader r = new StreamReader("sources.json"))
            {
                string json = r.ReadToEnd();
                IndexSources = Newtonsoft.Json.JsonConvert.DeserializeObject<IndexSource[]>(json);
            }

            bool result = RunAsync().GetAwaiter().GetResult();
            if (!result && !DebugMode)
            {
                Console.WriteLine("Something went wrong.  Set 'DebugMode' to true in order to see traces.");
            }
            else if (!result)
            {
                Console.WriteLine("Something went wrong.");
            }
            else
            {
                Console.WriteLine("All operations were successful.");
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static async Task<bool> RunAsync()
        {
            bool result = await DeleteIndexingResources();
            if (!result)
                return result;
            result = await CreateBlobContainerForImageStore();
            if (!result)
                return result;
            result = await CreateDataSource();
            if (!result)
                return result;
            result = await CreateSkillSet();
            if (!result)
                return result;
            result = await CreateSynonyms();
            if (!result)
                return result;
            result = await CreateIndex();
            if (!result)
                return result;
            result = await CreateIndexer();
            if (!result)
                return result;
            if (ShouldDeployWebsite)
            {
                result = await DeployWebsite();
            }
            result = await CheckIndexerStatus();
            if (!result)
                return result;
            result = await QueryIndex();
            return result;
        }

        private static async Task<bool> DeleteIndexingResources()
        {
            Console.WriteLine("Deleting Data Source, Index, Indexer and SynonymMap if they exist...");
            try
            {
                var sources = _searchClient.DataSources.List();
                foreach (var source in sources.DataSources)
                    await _searchClient.DataSources.DeleteAsync(source.Name);
                await _searchClient.Indexes.DeleteAsync(IndexName);
                var indexers = _searchClient.Indexers.List();
                foreach (var indexer in indexers.Indexers)
                    await _searchClient.Indexers.DeleteAsync(indexer.Name);
                await _searchClient.SynonymMaps.DeleteAsync(SynonymMapName);
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error deleting resources: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateBlobContainerForImageStore()
        {
            Console.WriteLine("Creating Blob Container for Image Store Skill...");
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlobStorageAccountConnectionString"]);
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(BlobContainerNameForImageStore);
                await container.CreateIfNotExistsAsync();
                // Note that setting this permission means that the container will be publically accessible.  This is necessary for
                // the website to work properly.  Remove these next 3 lines if you start using this code to process any private or
                // confidential data, but note that the website will stop working properly if you do.
                BlobContainerPermissions permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                await container.SetPermissionsAsync(permissions);
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating blob container: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateDataSource()
        {
            Console.WriteLine("Creating Data Source...");
            try
            {
                //DataSource dataSource = DataSource.AzureBlobStorage(
                //    name: DataSourceName,
                //    storageConnectionString: ConfigurationManager.AppSettings["JFKFilesBlobStorageAccountConnectionString"],
                //    containerName: ConfigurationManager.AppSettings["JFKFilesBlobContainerName"],
                //    description: "Data source for cognitive search example"
                //);

                foreach (var source in IndexSources)
                {
                    var azSource = new DataSource { Type = source.Type, Container = new DataContainer(source.Container), Credentials = new DataSourceCredentials(source.Credentials), Name = source.Name };
                    await _searchClient.DataSources.CreateAsync(azSource);
                }

            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating data source: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateSkillSet()
        {
            Console.WriteLine("Creating Skill Set...");
            try
            {
                if (_azureFunctionHostKey == null)
                {
                    _azureFunctionHostKey = await KeyHelper.GetAzureFunctionHostKey(_httpClient);
                }
                var skillsets = new[] { new { type = FileSkillsetName, file = "skillset.json" }, new { type = TextSkillsetName, file = "textSkillset.json" } };
                foreach (var skillset in skillsets)
                {
                    using (StreamReader r = new StreamReader(skillset.file))
                    {
                        string json = r.ReadToEnd();
                        json = json.Replace("[AzureFunctionEndpointUrl]", String.Format("https://{0}.azurewebsites.net", ConfigurationManager.AppSettings["AzureFunctionSiteName"]));
                        json = json.Replace("[AzureFunctionDefaultHostKey]", _azureFunctionHostKey);
                        json = json.Replace("[BlobContainerName]", BlobContainerNameForImageStore);
                        json = json.Replace("[CognitiveServicesKey]", ConfigurationManager.AppSettings["CognitiveServicesAccountKey"]);
                        string uri = String.Format("{0}/skillsets/{1}?api-version={2}", _searchServiceEndpoint, skillset.type, _searchApiVersion);
                        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await _httpClient.PutAsync(uri, content);
                        if (DebugMode)
                        {
                            string responseText = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Create Skill Set response: \n{0}", responseText);
                        }
                        if (!response.IsSuccessStatusCode)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating skillset: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateSynonyms()
        {
            Console.WriteLine("Creating Synonym Map...");
            try
            {
                SynonymMap synonyms = new SynonymMap(SynonymMapName,
                    @"GPFLOOR,oswold,ozwald,ozwold,oswald
                      silvia, sylvia
                      sever, SERVE, SERVR, SERVER
                      novenko, nosenko, novenco, nosenko");
                await _searchClient.SynonymMaps.CreateAsync(synonyms);
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating synonym map: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateIndex()
        {
            Console.WriteLine("Creating Index...");
            try
            {
                using (StreamReader r = new StreamReader("index.json"))
                {
                    string json = r.ReadToEnd();
                    json = json.Replace("[SynonymMapName]", SynonymMapName);
                    string uri = String.Format("{0}/indexes/{1}?api-version={2}", _searchServiceEndpoint, IndexName, _searchApiVersion);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PutAsync(uri, content);
                    if (DebugMode)
                    {
                        string responseText = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Create Index response: \n{0}", responseText);
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating index: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateIndexer()
        {
            Console.WriteLine("Creating Indexer...");
            try
            {
                foreach (var sourceType in IndexSources)
                {
                    var indexer = new Indexer();
                    indexer.Name = IndexerPrefix + sourceType.Name;
                    indexer.DataSourceName = sourceType.Name;
                    indexer.TargetIndexName = IndexName;
                    indexer.SkillsetName = sourceType.Type == DataSourceType.AzureBlob ? FileSkillsetName : TextSkillsetName;

                    if (sourceType.Type == DataSourceType.AzureBlob)
                    {
                        indexer.Parameters = new IndexingParameters { BatchSize = 1, MaxFailedItems = 0, MaxFailedItemsPerBatch = 0 };
                        var config = new Dictionary<string, object> {
                                { "dataToExtract", "contentAndMetadata" },
                                { "imageAction", "generateNormalizedImages" },
                                { "normalizedImageMaxWidth", 2000 },
                                { "normalizedImageMaxHeight", 2000 } };
                        indexer.Parameters.Configuration = config;

                        //"batchSize": 1,
                        //"maxFailedItems": 0,
                        //"maxFailedItemsPerBatch": 0,
                        //"configuration": {
                        //    "dataToExtract": "contentAndMetadata",
                        //    "imageAction": "generateNormalizedImages",
                        //    "normalizedImageMaxWidth": 2000,
                        //    "normalizedImageMaxHeight": 2000
                        //}

                    }
                    indexer.FieldMappings = sourceType.InputMapping;
                    indexer.OutputFieldMappings = sourceType.OutputMapping;
                    var response = await _searchClient.Indexers.CreateAsync(indexer);
                }

            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error creating indexer: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> DeployWebsite()
        {
            try
            {
                Console.WriteLine("Setting Website Keys...");
                string searchQueryKey = ConfigurationManager.AppSettings["SearchServiceQueryKey"];
                if (_azureFunctionHostKey == null)
                {
                    _azureFunctionHostKey = await KeyHelper.GetAzureFunctionHostKey(_httpClient);
                }
                string envText = File.ReadAllText("../../../../frontend/.env");
                envText = envText.Replace("[SearchServiceName]", ConfigurationManager.AppSettings["SearchServiceName"]);
                envText = envText.Replace("[SearchServiceDomain]", _searchClient.SearchDnsSuffix);
                envText = envText.Replace("[IndexName]", IndexName);
                envText = envText.Replace("[SearchServiceApiKey]", searchQueryKey);
                envText = envText.Replace("[SearchServiceApiVersion]", _searchApiVersion);
                envText = envText.Replace("[AzureFunctionName]", ConfigurationManager.AppSettings["AzureFunctionSiteName"]);
                envText = envText.Replace("[AzureFunctionDefaultHostKey]", _azureFunctionHostKey);
                File.WriteAllText("../../../../frontend/.env", envText);

                Console.WriteLine("Website keys have been set.  Please build the website and then return here and press any key to continue.");
                Console.ReadKey();

                Console.WriteLine("Deploying Website...");
                if (File.Exists("website.zip"))
                {
                    File.Delete("website.zip");
                }
                ZipFile.CreateFromDirectory("../../../../frontend/dist", "website.zip");
                byte[] websiteZip = File.ReadAllBytes("website.zip");
                HttpContent content = new ByteArrayContent(websiteZip);
                string uri = String.Format("https://{0}.scm.azurewebsites.net/api/zipdeploy?isAsync=true", ConfigurationManager.AppSettings["AzureWebAppSiteName"]);

                byte[] credentials = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", ConfigurationManager.AppSettings["AzureWebAppUsername"], ConfigurationManager.AppSettings["AzureWebAppPassword"]));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

                HttpResponseMessage response = await _httpClient.PostAsync(uri, content);
                if (DebugMode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Deploy website response: \n{0}", responseText);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                Console.WriteLine("Website deployment accepted.  Waiting for deployment to complete...");
                IEnumerable<string> values;
                if (response.Headers.TryGetValues("Location", out values))
                {
                    string pollingUri = values.First();
                    bool complete = false;
                    while (!complete)
                    {
                        Thread.Sleep(3000);
                        HttpResponseMessage pollingResponse = await _httpClient.GetAsync(pollingUri);
                        string responseText = await pollingResponse.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(responseText);
                        complete = json.SelectToken("complete") == null ? false : json.SelectToken("complete").ToObject<bool>();
                        if (DebugMode)
                        {
                            Console.WriteLine("Current website deployment status: {0}", json.SelectToken("progress")?.ToString());
                        }
                    }
                    Console.WriteLine("Website deployment completed.");
                }
                else
                {
                    Console.WriteLine("Could not find polling url from response.");
                }
                Console.WriteLine("Website url: https://{0}.azurewebsites.net/", ConfigurationManager.AppSettings["AzureWebAppSiteName"]);
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error deploying website: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }

        private static async Task<bool> CheckIndexerStatus()
        {
            Console.WriteLine("Waiting for indexing to complete...");
            IndexerExecutionStatus requestStatus = IndexerExecutionStatus.InProgress;
            try
            {
                //await _searchClient.Indexers.GetAsync(IndexerName);
                var allIndexers = IndexSources.Select(source => Task.Run(async () =>
                {
                    while (requestStatus.Equals(IndexerExecutionStatus.InProgress))
                    {
                        Thread.Sleep(3000);
                        IndexerExecutionInfo info = await _searchClient.Indexers.GetStatusAsync(IndexerPrefix + source.Name);
                        requestStatus = info.LastResult.Status;
                        if (DebugMode)
                        {
                            Console.WriteLine("Current indexer status: {0}", requestStatus.ToString());
                        }
                    }
                }));

                await Task.WhenAll(allIndexers.ToArray());
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error retrieving indexer status: {0}", ex.Message);
                }
                return false;
            }
            return requestStatus.Equals(IndexerExecutionStatus.Success);
        }

        private static async Task<bool> QueryIndex()
        {
            Console.WriteLine("Querying Index...");
            try
            {
                ISearchIndexClient indexClient = _searchClient.Indexes.GetClient(IndexName);
                DocumentSearchResult<Document> searchResult = await indexClient.Documents.SearchAsync("*");
                Console.WriteLine("Query Results:");
                foreach (var result in searchResult.Results)
                {
                    foreach (string key in result.Document.Keys)
                    {
                        Console.WriteLine("{0}: {1}", key, result.Document[key]);
                    }
                }
            }
            catch (Exception ex)
            {
                if (DebugMode)
                {
                    Console.WriteLine("Error querying index: {0}", ex.Message);
                }
                return false;
            }
            return true;
        }
    }
}
