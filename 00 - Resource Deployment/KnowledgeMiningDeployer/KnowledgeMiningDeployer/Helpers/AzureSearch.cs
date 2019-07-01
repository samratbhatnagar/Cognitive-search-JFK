using KnowledgeMiningDeployer.Classes;
using KnowledgeMiningDeployer.Helpers;
using KnowledgeMiningDeployer.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer
{
    public class AzureSearch
    {
        SearchCredentials creds;
        SearchIndexClient searchIndexClient;
        SearchServiceClient searchClient;

        public Hashtable Properties { get; set; }

        public string ApiVersion { get; set; }

        public AzureSearch()
        {
            try
            {
                creds = new Microsoft.Azure.Search.SearchCredentials(Configuration.SearchKey);
                searchIndexClient = new Microsoft.Azure.Search.SearchIndexClient(creds);
                searchClient = new SearchServiceClient(creds);

                searchClient.SearchServiceName = Configuration.SearchServiceName;
                searchIndexClient.SearchServiceName = Configuration.SearchServiceName;

                ApiVersion = Configuration.SearchServiceApiVersion;

                CreateIndexes(false);
            }
            catch (Exception ex)
            {

            }
        }

        public void AddAzureDataSource(string type)
        {

        }

        public void AddAzureDataSource_StorageAccount_Table(dynamic config)
        {
            //enumerate all properties...

            //insert all data...
        }

        public void AddAzureDataSource_StorageAccount_Blob(string name, string connectionString, string containerName)
        {
            BlobStorageConfig c = new BlobStorageConfig();
            c.AccountName = name;
            c.ConnectionString = connectionString;
            c.ContainerName = containerName;
            c.SasToken = "";

            try
            {
                GetOrCreateBlobDataSource(searchClient, name.ToString(), DataSourceType.AzureBlob, c);

                //enumrate properties in blob storage...

                //create an indexer for the data source
                var indexer = GetIndexerFromFile("base-indexer");
                indexer.Name = name;
                indexer.DataSourceName = name;
                indexer.TargetIndexName = "base";
                indexer.SkillsetName = "base";

                DeleteIndexerIfExists(searchClient, indexer.Name);

                CreateIndexer(searchClient, indexer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void AddAzureDataSource_StorageAccount_Blob(dynamic config)
        {
            try
            {
                BlobStorageConfig c = new BlobStorageConfig();
                c.AccountName = config.AccountName;
                c.ConnectionString = config.ConnectionString;
                c.ContainerName = config.ContainerName;
                c.SasToken = config.SasToken;

                GetOrCreateBlobDataSource(searchClient, config.name.ToString(), DataSourceType.AzureBlob, c);

                //TODO: enumrate properties in blob storage...dyanmically add to index...

                //create an indexer for the data source
                var indexer = GetIndexerFromFile(config.indexer.ToString());
                indexer.Name = config.name.ToString();
                indexer.DataSourceName = config.name.ToString();
                indexer.TargetIndexName = config.targetIndex.ToString();
                indexer.SkillsetName = config.skillset.ToString();

                DeleteIndexerIfExists(searchClient, indexer.Name);

                CreateIndexer(searchClient, indexer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddAzureDataSource_CosmosDb(dynamic config)
        {
            CosmosDbConfig dbConfig = new CosmosDbConfig();
            dbConfig.ConnectionString = config.connectionString;
            dbConfig.ContainerId = config.containerId;
            dbConfig.DatabaseId = config.databaseId;
            
            GetOrCreateCosmosDataSource(searchClient, config.name.ToString(), DataSourceType.CosmosDb, dbConfig);

            //enumerate all properties of master document...

            try
            {
                //create an indexer for the data source
                var indexer = GetIndexerFromFile(config.indexer.ToString());
                indexer.Name = config.name.ToString();
                indexer.DataSourceName = config.name.ToString();
                indexer.TargetIndexName = config.targetIndex.ToString();
                indexer.SkillsetName = "base";

                DeleteIndexerIfExists(searchClient, indexer.Name);

                CreateIndexer(searchClient, indexer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddAzureDataSource_AzureSql(dynamic config)
        {
            SqlDbConfig dbConfig = new SqlDbConfig();
            dbConfig.ConnectionString = config.connectionstring;
            dbConfig.Table = config.table;

            //add the database...
            GetOrCreateAzureSqlDataSource(searchClient, config.name.ToString(), DataSourceType.AzureSql, dbConfig);

            //TODO - add an indexer...

            //https://docs.microsoft.com/en-us/azure/search/search-howto-connecting-azure-sql-database-to-azure-search-using-indexers
        }

        public void CreateIndexes(bool reset)
        {
            dynamic data = new System.Dynamic.ExpandoObject();

            AddIndex("application", data, "ApplicationId", null);
        }

        private void AddIndex(string name, dynamic obj, string key, object blah)
        {
            
        }

        public WebApiSkill CreateWebApiSkill(dynamic config)
        {
            // Create the custom translate skill
            WebApiSkill skill = new WebApiSkill
            {
                Description = config.Description,
                Context = config.Context,
                Uri = config.Uri,
                HttpMethod = config.HttpMethod,
                //HttpHeaders = new WebApiHttpHeaders(), // This is broken in the SDK, so handle by sending JSON directly to Rest API.
                BatchSize = 1,
                Inputs = new List<InputFieldMappingEntry>(),
                Outputs = new List<OutputFieldMappingEntry>()
            };

            foreach(dynamic inputField in config.InputFields)
            {
                InputFieldMappingEntry f = new InputFieldMappingEntry(inputField.Name, inputField.Path);
                skill.Inputs.Add(f);
            }

            foreach(dynamic outField in config.OutputFields)
            {
                OutputFieldMappingEntry f = new OutputFieldMappingEntry(outField.Name, outField.Path);
                skill.Outputs.Add(f);
            }

            return skill;
        }

        async internal void CreateKnowledgeSkillSet(dynamic config)
        {
            Console.WriteLine($"Creating knowledge skillset...");

            DeleteSkillsetIfExists(searchClient, config.name.ToString());

            KnowledgeStoreSkillset skillset = GetKnowledgeSkillsetFromConfig(config);

            TokenReplace(skillset);

            WarmUpSkills(skillset);

            //supported knowledge store...
            this.ApiVersion = "2019-05-06-Preview";

            // Currently, the SDK is broken in how it handles Http Headers, so if using a custom WebApiSkill, send to the REST API, and don't use the SDK service.
            Skillset ss = await CreateSkillsetViaApi(skillset);

            if (ss != null)
                Console.WriteLine($"{ss.Name} was created");
            //    CreateSkillset(searchClient, skillset);

        }

        private void TokenReplace(Skillset skillset)
        {
            foreach(Skill s in skillset.Skills)
            {
                WebApiSkill api = s as WebApiSkill;

                if ( api != null)
                {
                    if ( Properties.ContainsKey("functionUrl"))
                        api.Uri = api.Uri.Replace("{functionurl}", Properties["functionUrl"].ToString() + "/api") + "?code=" + Properties["functionUrlKey"];

                    //set to 90 sec timeout
                    api.Timeout = new TimeSpan(0,0,90);
                }
            }
        }

        private void WarmUpSkills(Skillset skillset)
        {
            foreach (Skill s in skillset.Skills)
            {
                WebApiSkill api = s as WebApiSkill;

                if (api != null)
                {
                    //do a simple get...even if it errors...
                    HttpHelper hh = new HttpHelper(api.Uri);

                    try
                    {
                        hh.GetResponse();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting [{api.Uri}] : [{ex.Message}]");
                    }
                }
            }
        }

        private KnowledgeStoreSkillset GetKnowledgeSkillsetFromConfig(dynamic config)
        {
            string json = JsonConvert.SerializeObject(config);

            //quick token replace...
            json = json.Replace("{blobContainerName}", Configuration.StorageContainer);

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            serializerSettings.Converters.Add(new IsoDateTimeConverter());
            serializerSettings.Converters.Add(new PolymorphicDeserializeJsonConverter<Skill>("@odata.type"));
            serializerSettings.Converters.Add(new PolymorphicDeserializeJsonConverter<WebApiSkill>("@odata.type"));

            var skillset = JsonConvert.DeserializeObject<KnowledgeStoreSkillset>(json, serializerSettings);
            skillset.Name = config.name.ToString().ToLower();
            skillset.Description = "Knowledgestore skills collection";
            skillset.CognitiveServices = new CognitiveServicesByKey(Configuration.CognitiveServicesKey, Configuration.CognitiveServicesResourceId);
            skillset.knowledgeStore = new System.Dynamic.ExpandoObject();
            skillset.knowledgeStore.storageConnectionString = config.knowledgeStore.storageConnectionString;
            skillset.knowledgeStore.projections = config.knowledgeStore.projections;

            return skillset;
        }

        async internal void CreateSkillSet(dynamic config)
        {
            Console.WriteLine($"Creating skillset...");

            DeleteSkillsetIfExists(searchClient, config.name.ToString());

            //add index fields
            //AddSkillFields();

            Skillset skillset = GetSkillsetFromConfig(config);

            TokenReplace(skillset);

            WarmUpSkills(skillset);

            // Currently, the SDK is broken in how it handles Http Headers, so if using a custom WebApiSkill, send to the REST API, and don't use the SDK service.
            if (skillset.Skills != null && skillset.Skills.Any(s => s.GetType().Name == "WebApiSkill"))
            {
                Skillset ss = await CreateSkillsetViaApi(skillset);

                if (ss != null)
                    Console.WriteLine($"{ss.Name} was created");
            }
            else
            {
                Skillset ss = CreateSkillset(searchClient, skillset);

                if (ss != null)
                    Console.WriteLine($"{ss.Name} was created");
            }

        }

        internal async void CreateSkillSet(string name)
        {
            DeleteSkillsetIfExists(searchClient, name);

            var skillset = await GetSkillsetFromFile(name);

            // Create a new Index, Indexer and Skillset.
            // Currently, the SDK is broken in how it handles Http Headers, so if using a custom WebApiSkill, send to the REST API, and don't use the SDK service.
            if (skillset.Skills.Any(s => s.GetType().Name == "WebApiSkill"))
            {
                Skillset ss = await CreateSkillsetViaApi(skillset);

                if (ss != null)
                    Console.WriteLine("Skillset created");
            }
            else
            {
                CreateSkillset(searchClient, skillset);
            }

        }

        internal void CreateDataSourcesFromConfiguration(dynamic config)
        {
            Console.WriteLine($"Enumerating and creating data sources from Configuration File");

            //create the data sources from configuration
            foreach (dynamic ds in config.datasources)
            {
                switch (ds.type.ToString().ToLower())
                {
                    case "sqlserver":
                        AddAzureDataSource_AzureSql(ds);
                        break;
                    case "table":
                        AddAzureDataSource_StorageAccount_Table(ds);
                        break;
                    case "blob":
                        AddAzureDataSource_StorageAccount_Blob(ds);
                        break;
                    case "cosmosdb":
                        AddAzureDataSource_CosmosDb(ds);
                        break;
                }
            }
        }

        internal void CreateDataSources()
        {
            Console.WriteLine($"Enumerating and creating data sources from Azure Resource Group");

            //add each storage account...
            foreach (var sa in AzureHelper.AzureInstance.StorageAccounts.List())
            {
                if (sa.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    BlobStorageConfig c = new BlobStorageConfig();
                    c.AccountName = sa.Name + "blob";
                    string key = sa.GetKeys()[0].Value;
                    string connString = $"DefaultEndpointsProtocol=https;AccountName={sa.Name};AccountKey={key};EndpointSuffix=core.windows.net";
                    c.ConnectionString = connString;
                    c.ContainerName = "documents";
                    c.SasToken = "";

                    //add each storage account - blob
                    GetOrCreateBlobDataSource(searchClient, c.AccountName, DataSourceType.AzureBlob, c);

                    //create an indexer for the data source
                    var indexer = GetIndexerFromFile("base-indexer");
                    indexer.Name = "base-indexer-" + c.AccountName;
                    indexer.DataSourceName = c.AccountName;
                    indexer.TargetIndexName = "base-index";
                    indexer.SkillsetName = "base";

                    DeleteIndexerIfExists(searchClient, indexer.Name);

                    CreateIndexer(searchClient, indexer);

                    //add each storage account - table
                    c.AccountName = sa.Name + "table";
                    key = sa.GetKeys()[0].Value;
                    connString = $"DefaultEndpointsProtocol=https;AccountName={sa.Name};AccountKey={key};EndpointSuffix=core.windows.net";
                    c.ConnectionString = connString;
                    c.ContainerName = "documents";
                    c.SasToken = "";

                    //add each storage account - blob
                    GetOrCreateBlobDataSource(searchClient, c.AccountName, DataSourceType.AzureTable, c);

                }
            }

            //add each cosmosdb
            foreach(var sa in AzureHelper.AzureInstance.CosmosDBAccounts.List())
            {
                if (sa.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    CosmosDbConfig dbConfig = new CosmosDbConfig();
                    dbConfig.ConnectionString = sa.ListConnectionStrings().ConnectionStrings[0].ConnectionString;
                    dbConfig.ContainerId = "documents";
                    dbConfig.DatabaseId = "documents";

                    //add the cosomos db account
                    GetOrCreateCosmosDataSource(searchClient, sa.Name, DataSourceType.CosmosDb, dbConfig);
                }
            }

            //add each azure sql server - https://docs.microsoft.com/en-us/azure/search/search-howto-connecting-azure-sql-database-to-azure-search-using-indexers
            foreach (var sa in AzureHelper.AzureInstance.SqlServers.List())
            {
                if (sa.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    foreach(var db in sa.Databases.List())
                    {
                        SqlDbConfig config = new SqlDbConfig();
                        config.ConnectionString = $"Server={sa.FullyQualifiedDomainName};Database={db.Name};Uid={sa.AdministratorLogin},Pwd={""}";
                        config.Table = "documents";

                        //add the database...
                        GetOrCreateAzureSqlDataSource(searchClient, db.Name, DataSourceType.AzureSql, config);
                    }
                }
            }
        }

        internal void CreateIndexer(string name)
        {
            var indexer = GetIndexerFromFile(name);
            CreateIndexer(searchClient, indexer);
        }

        internal void CreateBase(SearchConfig sc)
        {
            CreateIndex("base", "base-index");
        }

        public void CreateIndex(string name, string fileName)
        {
            Console.WriteLine($"Creating index {name} from {fileName}");

            try
            {
                var index = GetIndexFromFile(fileName);
                index.Name = name;

                CreateCognitiveSearchPipeline(searchClient, index, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        List<Field> GetFields(object obj, string keyField, Hashtable sortableFields, Hashtable refinerFields)
        {
            List<Field> fields = new List<Field>();

            PropertyInfo[] pis = obj.GetType().GetProperties();

            foreach (PropertyInfo pi in pis)
            {
                Field f = new Field(pi.Name, DataType.String);

                bool add = false;

                switch (pi.PropertyType.Name)
                {
                    case "String":
                        f.Type = "String";
                        f.IsSearchable = true;
                        f.IsFilterable = true;
                        add = true;
                        break;
                    case "DateTime":
                        f.Type = "DateTimeOffset";
                        add = true;
                        break;
                    case "Boolean":
                        f.Type = "Boolean";
                        add = true;
                        break;
                    case "Int32":
                        f.Type = "Int32";
                        add = true;
                        break;
                    case "Int64":
                        f.Type = "Int64";
                        add = true;
                        break;
                    case "ICollection`1":
                        add = false;
                        break;
                    default:
                        f.Type = "String";
                        f.IsSearchable = true;
                        f.IsFilterable = true;
                        add = true;
                        break;
                }

                if (f.Name == keyField)
                    f.IsKey = true;

                if (add)
                    fields.Add(f);
            }

            bool found = false;
            foreach (Field f in fields)
            {
                if (f.Name == "Type")
                    found = true;
            }

            if (!found)
            {
                Field ft = new Field("Type", DataType.String);
                ft.IsSearchable = true;
                ft.IsFilterable = true;
                fields.Add(ft);
            }

            return fields;
        }

        private static void AddSentimentAnalysisSkill(ref Index index, ref Indexer indexer, ref Skillset skillset)
        {
            if (index.Fields.Any(f => f.Name == "sentiment"))
            {
                return;
            }

            index.Fields.Add(new Field("sentiment", DataType.Double));
            indexer.OutputFieldMappings.Add(CreateFieldMapping("document/sentiment", "sentiment").GetAwaiter().GetResult());
            skillset.Skills.Add(new SentimentSkill
            {
                Context = "/document",
                Description = "Sentiment analysis skill",
                DefaultLanguageCode = "en",
                Inputs = new List<InputFieldMappingEntry> { new InputFieldMappingEntry("text", "/document/text") },
                Outputs = new List<OutputFieldMappingEntry> { new OutputFieldMappingEntry("score", "sentiment") }
            });
        }

        private static void AddUserInfoToIndex(ref Index index, ref Indexer indexer)
        {
            var analyzer = AnalyzerName.StandardLucene;
            // Create a new index fields for userName and userLocation
            index.Fields.Add(new Field("userName", analyzer));
            index.Fields.Add(new Field("userLocation", analyzer));

            indexer.OutputFieldMappings.Add(CreateFieldMapping("/document/user/name", "userName").GetAwaiter().GetResult());
            indexer.OutputFieldMappings.Add(CreateFieldMapping("/document/user/location", "userLocation").GetAwaiter().GetResult());
        }

        private static void AddCustomTranslateSkill(ref Index index, ref Indexer indexer, ref Skillset skillset, FunctionAppConfig config)
        {
            var targetField = "textTranslated";
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };

            index.Fields.Add(new Field(targetField, AnalyzerName.StandardLucene));
            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/{targetField}", targetField).GetAwaiter().GetResult());

            // Create the custom translate skill
            skillset.Skills.Add(new WebApiSkill
            {
                Description = "Custom translator skill",
                Context = "/document",
                Uri = $"{config.Url}/api/Translate?code={config.DefaultHostKey}",
                HttpMethod = "POST",
                //HttpHeaders = new WebApiHttpHeaders(headers),
                BatchSize = 1,
                Inputs = new List<InputFieldMappingEntry>
                {
                    new InputFieldMappingEntry("text", "/document/text"),
                    new InputFieldMappingEntry("language", "/document/language")
                },
                Outputs = new List<OutputFieldMappingEntry>
                {
                    new OutputFieldMappingEntry("text", targetField)
                }
            });

            // Update all the other skills, except for the LanguageDetectionSkill, to use the new textTranslated field.
            foreach (var skill in skillset.Skills)
            {
                var type = skill.GetType();
                var typeName = type.Name;

                if (typeName != "WebApiSkill" && typeName != "LanguageDetectionSkill")
                {
                    foreach (var input in skill.Inputs)
                    {
                        if (input.Source == "/document/text")
                        {
                            input.Source = $"/document/{targetField}";
                        }
                    }
                }
            }
        }

        private static async Task CreateFormsRecognitionPipeline(SearchServiceClient searchClient, AppConfig appConfig, string modelId)
        {
            var formsSearchConfig = new SearchConfig
            {
                DataSourceName = "forms-datasource",
                IndexName = "forms-index",
                IndexerName = "forms-indexer",
                SkillsetName = "forms-skillset"
            };

            var formsDataSource = GetOrCreateBlobDataSource(searchClient, formsSearchConfig.DataSourceName, DataSourceType.AzureBlob, appConfig.BlobStorage);
            Console.WriteLine($"Successfully created data source {formsSearchConfig.DataSourceName}");

            var formsIndex = GetIndexFromFile(formsSearchConfig.IndexName);
            var formsIndexer = GetIndexerFromFile(formsSearchConfig.IndexerName, formsSearchConfig);
            var formsSkillset = await GetSkillsetFromFile(formsSearchConfig.SkillsetName);

            Console.WriteLine("Adding Custom Form Recognizer skill to pipeline");
            AddCustomFormRecognizerSkill(ref formsIndex, ref formsIndexer, ref formsSkillset, appConfig, modelId);

            CreateCognitiveSearchPipeline(searchClient, formsIndex, formsIndexer);

            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static async Task<string> TrainFormRecognizerModel(FormRecognizerConfig formConfig, BlobStorageConfig storageConfig)
        {
            var formRecognizerTrainUri = $"{formConfig.Endpoint}formrecognizer/v1.0-preview/custom/train";
            var sasUri = $"https://{storageConfig.AccountName}.blob.core.windows.net/{storageConfig.ContainerName}/{storageConfig.SasToken}";
            var requestBody = JsonConvert.SerializeObject(new FormRecognizerTrainRequestBody { Source = sasUri });

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(formRecognizerTrainUri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", formConfig.Key);

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = JsonConvert.DeserializeObject<FormRecognizerErrorResponse>(responseBody);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(errorResponse.Error.Message);
                        return "";
                    }

                    var successResponse = JsonConvert.DeserializeObject<FormRecognizerTrainSuccessResponse>(responseBody);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successfully trained the form recognizer model with {successResponse.TrainingDocuments.Count} forms.");
                    Console.ResetColor();

                    return successResponse.ModelId;
                }
            }
        }

        private static void AddCustomFormRecognizerSkill(ref Index index, ref Indexer indexer, ref Skillset skillset, AppConfig config, string modelId)
        {
            var headers = new WebApiHttpHeaders(new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            });

            index.Fields.Add(new Field($"formHeight", DataType.Int32));
            index.Fields.Add(new Field($"formWidth", DataType.Int32));
            index.Fields.Add(new Field($"formKeyValuePairs", DataType.Collection(DataType.String)));
            index.Fields.Add(new Field($"formColumns", DataType.Collection(DataType.String)));
            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/formHeight", "formHeight").GetAwaiter().GetResult());
            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/formWidth", "formWidth").GetAwaiter().GetResult());
            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/formKeyValuePairs", "formKeyValuePairs").GetAwaiter().GetResult());
            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/formColumns", "formColumns").GetAwaiter().GetResult());

            // Create the custom translate skill
            skillset.Skills.Add(new WebApiSkill
            {
                Description = "Custom Form Recognizer skill",
                Context = "/document",
                Uri = $"{config.FunctionApp.Url}/api/AnalyzeForm?code={config.FunctionApp.DefaultHostKey}&modelId={modelId}",
                HttpMethod = "POST",
                //HttpHeaders = new WebApiHttpHeaders(), // This is broken in the SDK, so handle by sending JSON directly to Rest API.
                BatchSize = 1,
                Inputs = new List<InputFieldMappingEntry>
                {
                    new InputFieldMappingEntry("contentType", "/document/fileContentType"),
                    new InputFieldMappingEntry("storageUri", "/document/storageUri"),
                    new InputFieldMappingEntry("storageSasToken", "/document/sasToken")
                },
                Outputs = new List<OutputFieldMappingEntry>
                {
                    new OutputFieldMappingEntry("formHeight", "formHeight"),
                    new OutputFieldMappingEntry("formWidth", "formWidth"),
                    new OutputFieldMappingEntry("formKeyValuePairs", "formKeyValuePairs"),
                    new OutputFieldMappingEntry("formColumns", "formColumns"),

                }
            });
        }

        private static async Task CreateAnomalyDetectionPipeline(SearchServiceClient searchClient, AppConfig appConfig)
        {
            var searchConfig = new SearchConfig
            {
                DataSourceName = "telemetry-datasource",
                IndexName = "telemetry-index",
                IndexerName = "telemetry-indexer",
                SkillsetName = "telemetry-skillset"
            };

            var dataSource = await GetOrCreateCosmosDataSource(searchClient, searchConfig.DataSourceName, DataSourceType.CosmosDb, appConfig.CosmosDb);
            Console.WriteLine($"Successfully created data source {searchConfig.DataSourceName}");

            var index = GetIndexFromFile(searchConfig.IndexName);
            var indexer = GetIndexerFromFile(searchConfig.IndexerName, searchConfig);
            var skillset = new Skillset
            {
                Name = searchConfig.SkillsetName,
                Description = "Anomaly detection skills",
                CognitiveServices = new CognitiveServicesByKey(appConfig.CognitiveServices.Key, appConfig.CognitiveServices.ResourceId),
                Skills = new List<Skill>()
            };

            Console.WriteLine("Adding Custom Anomaly Detector skill to pipeline");
            AddCustomAnomalyDetectorSkill(ref index, ref indexer, ref skillset, appConfig);

            CreateCognitiveSearchPipeline(searchClient, index, indexer);

            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static void AddCustomAnomalyDetectorSkill(ref Index index, ref Indexer indexer, ref Skillset skillset, AppConfig config)
        {
            var headers = new WebApiHttpHeaders(new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            });

            var anomalyFields = new List<Field>
            {
                new Field($"isAnomaly", DataType.Boolean),
                new Field($"isPositiveAnomaly", DataType.Boolean),
                new Field($"isNegativeAnomaly", DataType.Boolean),
                new Field($"expectedValue", DataType.Double),
                new Field($"upperMargin", DataType.Double),
                new Field($"lowerMargin", DataType.Double)
            };
            index.Fields.Add(new Field("engineTemperatureAnalysis", DataType.Complex, anomalyFields));

            indexer.OutputFieldMappings.Add(CreateFieldMapping($"/document/engineTemperatureAnalysis", "engineTemperatureAnalysis").GetAwaiter().GetResult());

            // Create the custom translate skill
            skillset.Skills.Add(new WebApiSkill
            {
                Description = "Custom Anomaly Detector skill",
                Context = "/document",
                Uri = $"{config.FunctionApp.Url}/api/DetectAnomalies?code={config.FunctionApp.DefaultHostKey}",
                HttpMethod = "POST",
                //HttpHeaders = new WebApiHttpHeaders(), // This is broken in the SDK, so handle by sending JSON directly to Rest API.
                BatchSize = 1,
                Inputs = new List<InputFieldMappingEntry>
                {
                    new InputFieldMappingEntry("timestamp", "/document/timestamp"),
                    new InputFieldMappingEntry("engineTemperature", "/document/engineTemperature")
                },
                Outputs = new List<OutputFieldMappingEntry>
                {
                    new OutputFieldMappingEntry("anomalyResult", "engineTemperatureAnalysis")

                }
            });
        }

        private static async Task<bool> TrainAnomalyDetectorModel(AppConfig appConfig)
        {
            var anomalyDetectorTrainUri = $"{appConfig.AnomalyDetector.Endpoint}anomalydetector/v1.0/timeseries/entire/detect";

            var timeSeriesData = new Series
            {
                maxAnomalyRatio = 0.25F,
                sensitivity = 95,
                granularity = "minutely"
            };

            // Retrieve data from Cosmos DB
            var cosmosDbConnectionString = new CosmosDbConnectionString(appConfig.CosmosDb.ConnectionString);
            // Set the Cosmos DB connection policy.
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            using (var cosmosClient = new DocumentClient(cosmosDbConnectionString.ServiceEndpoint, cosmosDbConnectionString.AuthKey, connectionPolicy))
            {
                var containerUri = UriFactory.CreateDocumentCollectionUri(appConfig.CosmosDb.DatabaseId, appConfig.CosmosDb.ContainerId);
                var query = cosmosClient.CreateDocumentQuery<EngineTempRecord>(containerUri.ToString(),
                    new Microsoft.Azure.Documents.SqlQuerySpec("SELECT TOP 5000 c.engineTemperature FROM c"),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .ToList();

                var models = new List<AnomalyModel>();
                var startDateTime = DateTime.Now.AddDays(-6);
                var i = 0;
                foreach (var temp in query)
                {
                    models.Add(new AnomalyModel { timestamp = startDateTime.AddMinutes(i), value = temp.engineTemperature });
                    i++;
                }
                timeSeriesData.series = models;
            }

            var requestBody = JsonConvert.SerializeObject(timeSeriesData);

            // Send the training request to the Anomaly Detector batch detect enpoint.
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(anomalyDetectorTrainUri);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", appConfig.AnomalyDetector.Key);

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Console.ForegroundColor = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine(responseBody);
                    Console.ResetColor();

                    return response.IsSuccessStatusCode;
                }
            }
        }

        //https://docs.microsoft.com/en-us/azure/search/search-howto-indexing-azure-blob-storage
        public static DataSource GetOrCreateBlobDataSource(ISearchServiceClient serviceClient, string name, DataSourceType dataSourceType, BlobStorageConfig blobStorageConfig, string query = "")
        {
            
            if (serviceClient.DataSources.Exists(name))
            {
                //delete it
                serviceClient.DataSources.Delete(name);
            }

            var dataSource = new DataSource
            {
                Name = name,
                Type = dataSourceType,
                Credentials = new DataSourceCredentials(blobStorageConfig.ConnectionString),
                Container = new DataContainer(blobStorageConfig.ContainerName)
            };

            return serviceClient.DataSources.Create(dataSource);
        }

        public static async Task<DataSource> GetOrCreateCosmosDataSource(ISearchServiceClient serviceClient, string name, DataSourceType dataSourceType, CosmosDbConfig config, string query = "")
        {
            if (await serviceClient.DataSources.ExistsAsync(name))
            {
                return await serviceClient.DataSources.GetAsync(name);
            }

            var dataSource = new DataSource
            {
                Name = name,
                Type = dataSourceType,
                Credentials = new DataSourceCredentials($"{config.ConnectionString};Database={config.DatabaseId}"),
                Container = new DataContainer(config.ContainerId, "SELECT * FROM c WHERE c._ts > @HighWaterMark ORDER BY c._ts")
            };

            return await serviceClient.DataSources.CreateAsync(dataSource);
        }

        public static async Task<DataSource> GetOrCreateAzureSqlDataSource(ISearchServiceClient serviceClient, string name, DataSourceType dataSourceType, SqlDbConfig config, string query = "")
        {
            if (await serviceClient.DataSources.ExistsAsync(name))
            {
                return await serviceClient.DataSources.GetAsync(name);
            }

            var dataSource = new DataSource
            {
                Name = name,
                Type = dataSourceType,
                Credentials = new DataSourceCredentials(config.ConnectionString),
                Container = new DataContainer { Name = config.Table }
            };

            return await serviceClient.DataSources.CreateAsync(dataSource);
        }

        public static Index GetIndex(ISearchServiceClient serviceClient, string indexName)
        {
            if ( serviceClient.Indexes.Exists(indexName))
            {
                return serviceClient.Indexes.Get(indexName);
            }
            else
            {
                return GetIndexFromFile(indexName);
            }
        }

        public static Index GetIndexFromFile(string name)
        {
            using (var reader = new StreamReader($"Configuration/{name}.json"))
            {
                var json = reader.ReadToEnd();
                var index = JsonConvert.DeserializeObject<Index>(json);
                index.Name = name;

                return index;
            }
        }

        private static Index CreateIndex(ISearchServiceClient serviceClient, Index index)
        {
            return serviceClient.Indexes.Create(index);
        }

        private static void DeleteIndexIfExists(ISearchServiceClient serviceClient, string indexName)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }
        }

        public static Indexer GetIndexer(ISearchServiceClient serviceClient, SearchConfig config)
        {
            if (serviceClient.Indexers.Exists(config.IndexerName))
            {
                return serviceClient.Indexers.Get(config.IndexerName);
            }
            else
            {
                return GetIndexerFromFile(config.IndexerName, config);
            }
        }

        public static Indexer GetIndexerFromFile(string name)
        {
            using (var reader = new StreamReader($"Configuration/{name}.json"))
            {
                var json = reader.ReadToEnd();
                var indexer = JsonConvert.DeserializeObject<Indexer>(json);
                indexer.Description = "Search Indexer";

                return indexer;
            }
        }

        public static Indexer GetIndexerFromFile(string name, SearchConfig config)
        {
            using (var reader = new StreamReader($"Configuration/{name}.json"))
            {
                var json = reader.ReadToEnd();
                var indexer = JsonConvert.DeserializeObject<Indexer>(json);
                indexer.Name = name;
                indexer.DataSourceName = config.DataSourceName;
                indexer.SkillsetName = config.SkillsetName;
                indexer.TargetIndexName = config.IndexName;
                indexer.Description = "Search Indexer";

                return indexer;
            }
        }

        private static Indexer CreateIndexer(ISearchServiceClient serviceClient, Indexer indexer)
        {
            return serviceClient.Indexers.Create(indexer);
        }

        private static void DeleteIndexerIfExists(ISearchServiceClient serviceClient, string indexerName)
        {
            if (serviceClient.Indexers.Exists(indexerName))
            {
                serviceClient.Indexers.Delete(indexerName);
            }
        }

        public static async Task<FieldMapping> CreateFieldMapping(string sourceFieldName, string targetFieldName)
        {
            return await Task.FromResult(new FieldMapping
            {
                SourceFieldName = sourceFieldName,
                TargetFieldName = targetFieldName
            });
        }

        public static async Task<Skillset> GetSkillset(ISearchServiceClient serviceClient, string skillsetName, CognitiveServicesConfig cognitiveServicesConfig)
        {
            if (await serviceClient.Skillsets.ExistsAsync(skillsetName))
            {
                // Retrieve the skillset from the Search Service.
                var skillset = await serviceClient.Skillsets.GetAsync(skillsetName);
                return skillset;
            }
            else
            {
                // Retrieve the default skillset from a JSON file.
                return await GetSkillsetFromFile(skillsetName);
            }
        }

        public static Skillset GetWebApiSkillsetFromConfig(dynamic config)
        {
            string json = JsonConvert.SerializeObject(config);
            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            serializerSettings.Converters.Add(new PolymorphicDeserializeJsonConverter<WebApiSkill>("@odata.type"));

            var skillset = JsonConvert.DeserializeObject<Skillset>(json, serializerSettings);
            skillset.Name = config.name.ToString().ToLower();
            skillset.Description = "Cognitive skills collection";
            skillset.CognitiveServices = new CognitiveServicesByKey(Configuration.CognitiveServicesKey, Configuration.CognitiveServicesResourceId);

            return skillset;
        }

        public static Skillset GetSkillsetFromConfig(dynamic config)
        {
            string json = JsonConvert.SerializeObject(config);
            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            serializerSettings.Converters.Add(new PolymorphicDeserializeJsonConverter<Skill>("@odata.type"));
            
            var skillset = JsonConvert.DeserializeObject<Skillset>(json, serializerSettings);
            skillset.Name = config.name.ToString().ToLower();
            skillset.Description = "Cognitive skills collection";
            skillset.CognitiveServices = new CognitiveServicesByKey(Configuration.CognitiveServicesKey, Configuration.CognitiveServicesResourceId);

            return skillset;
        }

        public static async Task<Skillset> GetSkillsetFromFile(string name)
        {
            using (var reader = new StreamReader($"Configuration/{name}.json"))
            {
                var json = await reader.ReadToEndAsync();
                var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                serializerSettings.Converters.Add(new PolymorphicDeserializeJsonConverter<Skill>("@odata.type"));

                var skillset = JsonConvert.DeserializeObject<Skillset>(json, serializerSettings);
                skillset.Name = name;
                skillset.Description = "Cognitive skills collection";
                skillset.CognitiveServices = new CognitiveServicesByKey(Configuration.CognitiveServicesKey, Configuration.CognitiveServicesResourceId);

                return skillset;
            }
        }

        private static Skillset CreateSkillset(ISearchServiceClient serviceClient, Skillset skillset)
        {
            try
            {
                return serviceClient.Skillsets.Create(skillset);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Create a skillset by sending JSON to the REST API endpoint.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="skillset"></param>
        /// <returns></returns>
        private async Task<Skillset> CreateSkillsetViaApi(Skillset skillset)
        {
            skillset.Name = skillset.Name.Replace(" ", "").ToLower();

            // This function is necessary because currently the SDK fails when trying to create 
            var uri = new Uri($"https://{Configuration.SearchServiceName}.search.windows.net/skillsets/{skillset.Name}?api-version={this.ApiVersion}");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Add("api-key", Configuration.SearchKey);

                    using (var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Put;
                        var serializerSettings = new JsonSerializerSettings();
                        serializerSettings.Converters.Add(new PolymorphicSerializeJsonConverter<Skill>("@odata.type"));
                        serializerSettings.Converters.Add(new PolymorphicSerializeJsonConverter<CognitiveServices>("@odata.type"));
                        var payload = JsonConvert.SerializeObject(skillset, serializerSettings);
                        
                        // Remove problematic values
                        var cleanPayload = payload.Replace(",\"@odata.etag\":null", "").Replace("\"httpHeaders\":null,", "");

                        //timespans...
                        cleanPayload = cleanPayload.Replace("\"timeout\":\"00:01:30\"", "\"timeout\":\"PT90S\"");

                        var content = new StringContent(cleanPayload, Encoding.UTF8, "application/json");

                        var response = await (httpClient.PutAsync(uri, content));
                        var responseContent = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseContent);

                        if (response.IsSuccessStatusCode)
                        {
                            return JsonConvert.DeserializeObject<Skillset>(responseContent);
                        }
                        else
                        {
                            var error = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                            throw new Exception(error.Error.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static void DeleteSkillsetIfExists(ISearchServiceClient serviceClient, string skillsetName)
        {
            skillsetName = skillsetName.Replace(" ", "");

            if (serviceClient.Skillsets.Exists(skillsetName))
            {
                serviceClient.Skillsets.Delete(skillsetName);
            }
        }

        /// <summary>
        /// Creates the Index, Indexer, and Skillset for the cognitive search pipeline.
        /// </summary>
        public static void CreateCognitiveSearchPipeline(ISearchServiceClient searchClient, Index index, Indexer indexer)
        {
            try
            {
                // Delete the existing Index, Indexer and Skillset
                DeleteIndexIfExists(searchClient, index.Name);
                CreateIndex(searchClient, index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}