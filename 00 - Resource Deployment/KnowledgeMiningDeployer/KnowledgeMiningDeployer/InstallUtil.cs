using KnowledgeMiningDeployer.Classes;
using KnowledgeMiningDeployer.Helpers;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.CognitiveServices.Fluent;
using Microsoft.Azure.Management.CognitiveServices.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Search.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.SqlServer.Dac;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer
{
    public class InstallUtil
    {
        public static string RunPath { get; private set; }

        public static Hashtable Properties { get; set; }
        public static void Initialize()
        {
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().FullName);
            RunPath = fi.Directory.FullName;

            Properties = new Hashtable();
            SetProperty("functionUrl", "jfk-func01.azurewebsites.net");
            SetProperty("apiUrl", "jfk-web02.azurewebsites.net");
            SetProperty("uiUrl", "jfk-web01.azurewebsites.net");
        }

        static public void SetProperty(string name, string value)
        {
            if (Properties.ContainsKey(name))
                Properties[name] = value;
            else
                Properties.Add(name, value);
        }

        public static void DeployMain(string resourceName)
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            FileInfo fi = new FileInfo(di.Parent.FullName + @"/AzureTemplates/Main.json");

            Hashtable ht = new Hashtable();
            ht.Add("prefix", Configuration.ResourcePrefix);
            ht.Add("tenantId", Configuration.TenantId);

            dynamic parameters = new System.Dynamic.ExpandoObject();
            //((IDictionary<String, object>)parameters)["$schema"] = "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#";
            //parameters.contentVersion = "1.0.0.0";
            //parameters.parameters = new System.Dynamic.ExpandoObject();

            dynamic p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.AdminAzureClientId;
            parameters.azureAdminClientId = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.AdminAzureClientSecret;
            parameters.azureAdminClientSecret = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.Username;
            parameters.adminUsername = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.Password;
            parameters.adminPassword = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.ResourcePrefix;
            parameters.prefix = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.UseSampleData.ToString();
            parameters.useSampleData = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.BlobStorageConnectionString;
            parameters.blobStorageConnectionString = p;

            p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.ConfigFilePath;
            parameters.configFilePath = p;


            /*
            parameters.azureAdminClientId = Configuration.AdminAzureClientId;
            parameters.azureAdminClientSecret = Configuration.AdminAzureClientSecret;
            parameters.adminUsername = Configuration.Username;
            parameters.adminPassword = Configuration.Password;
            parameters.prefix = Configuration.ResourcePrefix;
            parameters.useSampleData = Configuration.UseSampleData;
            parameters.blobStorageConnectionString = Configuration.BlobStorageConnectionString;
            parameters.configFilePath = Configuration.ConfigFilePath;
            parameters.customDataZip = Configuration.CustomDataZip;
            parameters.bingEndPoint = Configuration.BingEndpoint;
            parameters.bingKey = Configuration.BingKey;
            */

            string data = JsonConvert.SerializeObject(parameters);

            AzureHelper.CreateDeployment(fi, data, ht, Configuration.ResourceGroupName);
        }

        private static void TestOutputs()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            FileInfo fi = new FileInfo(di.Parent.FullName + @"/AzureTemplates/outputtest.json");

            Hashtable ht = new Hashtable();
            ht.Add("prefix", Configuration.ResourcePrefix);
            ht.Add("tenantId", Configuration.TenantId);

            dynamic parameters = new System.Dynamic.ExpandoObject();
            
            dynamic p = new System.Dynamic.ExpandoObject();
            p.Value = Configuration.AdminAzureClientId;
            parameters.azureAdminClientId = p;

            string data = JsonConvert.SerializeObject(parameters);

            AzureHelper.CreateDeployment(fi, data, ht, Configuration.ResourceGroupName);
        }

        private static string GetSqlServerConnectionString(ISqlServer azureSQLServer, string database)
        {
            return $"Server=tcp:{azureSQLServer.FullyQualifiedDomainName},1433;Initial Catalog={database};Persist Security Info=False;User ID={azureSQLServer.AdministratorLogin};Password={Configuration.Password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        async public static Task Start()
        {
            Configuration.LoadWithMode(ConfigurationManager.AppSettings["Mode"]);

            AzureHelper.Initialize(Configuration.ResourceGroupName);

            //do quick check...try to connect to each data source...
            if (!CheckConfiguration())
                return;

            //deploy the main template - Not needed, done in the arm template itself which then calls this exe via DSC.
            if (Configuration.DeployMain && false)
                DeployMain(Configuration.ResourceGroupName);

            //set all the configuration values...
            Task t1 = LoadConfiguration();

            //check to see if we have to unzip the supporting files
            if (!Directory.Exists("/AzureTemplates") && File.Exists("AzureTemplates.zip"))
            {
                FileInfo zip = new FileInfo("AzureTemplates.zip");

                //unzip it...
                Utility.ExtractZipFile("AzureTemplates.zip", null, zip.DirectoryName + "\\" + zip.Name.Replace(zip.Extension, ""));
            }

            if (!Directory.Exists("/Configuration") && File.Exists("Configuration.zip"))
            {
                FileInfo zip = new FileInfo("Configuration.zip");

                //unzip it...
                Utility.ExtractZipFile("Configuration.zip", null, zip.DirectoryName + "\\" + zip.Name.Replace(zip.Extension, ""));
            }
            
            if (Configuration.UseSampleData)
            {
                Console.WriteLine($"Deploying sample content");

                UploadZipToStorageAccount(Configuration.StorageAccountName, Configuration.StorageAccountKey, Configuration.StorageContainer, "Deployment/JFK.zip");

                if (!string.IsNullOrEmpty(Configuration.CustomDataZip))
                {
                    Console.WriteLine($"Deploying custom content");

                    UploadZipToStorageAccount(Configuration.StorageAccountName, Configuration.StorageAccountKey, Configuration.StorageContainer, "Deployment/CustomDocuments.zip");
                }

                //TODO - add to cosmosdb

                //add to azure sql - this database already has the data in it...
                try
                {
                    Console.WriteLine($"Deploying sql server content");

                    ISqlServer sqlServer = GetSqlServer();
                    string databaseName = Configuration.ResourcePrefix + "-db";
                    string connString = GetSqlServerConnectionString(sqlServer, databaseName);
                    DeployDatabase(RunPath + @"\Deployment\Documents.bacpac", connString, databaseName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //deploy func apps
            IFunctionApp funcApp = GetFunctionApp(Configuration.ResourcePrefix + "-func01");

            if (funcApp == null || Configuration.DoDeployments)
            {
                funcApp = DeployFunctionApp("func01", "CognitiveSearch.Skills");

                //this has to be done before we deploy the zip...funcation app changes will blow away storage connection
                //wire up the config options
                //SetupFunctionApp(funcApp, "CognitiveSearch.Skills");

                //deploy the zip
                var profile = funcApp.GetPublishingProfile();
                DeployWebZip("CognitiveSearch.Skills", funcApp.Name, profile.GitUsername, profile.GitPassword);
            }            

            //get the url..
            SetProperty("functionUrl", funcApp.DefaultHostName);

            //get the master key...
            try
            {
                string key = funcApp.GetMasterKey();
                SetProperty("functionUrlKey", key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //deploy web apps
            IWebApp webApp = GetWebApp(Configuration.ResourcePrefix + "-web02");

            if (webApp == null || Configuration.DoDeployments)
                DeployWebApp("web02", "CognitiveSearch.API");

            if (webApp != null)
            {
                SetProperty("apiUrl",webApp.DefaultHostName);

                //wire up the config options
                SetupWebApp(webApp, "CognitiveSearch.API", Properties);
            }

            webApp = GetWebApp(Configuration.ResourcePrefix + "-web01");

            //deploy web apps
            if (webApp == null || Configuration.DoDeployments)
                webApp = DeployWebApp("web01", "CognitiveSearch.UI");

            if (webApp != null)
            {
                SetProperty("uiUrl",webApp.DefaultHostName);
                
                //wire up the config options
                SetupWebApp(webApp, "CognitiveSearch.UI", Properties);
            }

            //TODO BONUS - auto login - upload notebook - run notebook
            //deploy machine learning skills
            DeployMLApp("CognitiveSearch.ML.Skills", "");
           
            //create search indexes...
            AzureSearch search = new AzureSearch();
            search.Properties = Properties;

            SearchConfig sc = new SearchConfig();

            //create based on the data sources in configuration file
            dynamic config = Configuration.Export(ConfigurationManager.AppSettings["Mode"]);

            //indexes are independent of anything and can be created first
            foreach(dynamic index in config.Indexes)
            {
                //creates the indexes from configuration files
                search.CreateIndex(index.name.ToString(), index.configuration.ToString());
            }

            //skillsets must be created before data sources and indexers...
            //list of skill types - https://docs.microsoft.com/en-us/azure/search/cognitive-search-predefined-skills
            foreach (dynamic skillSet in config.SkillSets)
                search.CreateKnowledgeSkillSet(skillSet);

            //create based on existing data sources in resource group
            //search.CreateDataSources();

            //if they passed in a blob storage connection, then use that
            if (!string.IsNullOrEmpty(Configuration.BlobStorageConnectionString))
            {
                string name = Utility.ParseValue(Configuration.BlobStorageConnectionString, "AccountName=", ";");
                search.AddAzureDataSource_StorageAccount_Blob(name, Configuration.BlobStorageConnectionString, Configuration.StorageContainer);
            }

            search.CreateDataSourcesFromConfiguration(config);

            //TODO BONUS - Forms deployment model training
            //https://github.com/solliancenet/tech-immersion-data-ai/blob/master/lab-files/ai/2/PipelineEnhancer/Program.cs
            FormRecognition fr = new FormRecognition();

            //TODO BONUS - deploy the powerBI - PowerShell
            DeployPowerBI("", "");
        }

        private static bool CheckConfiguration()
        {
            try
            {
                //Check the Blob storage connection string...
                CloudStorageAccount storageAccount;
                CloudStorageAccount.TryParse(Configuration.BlobStorageConnectionString, out storageAccount);
                CloudBlobClient c = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = c.GetContainerReference(Configuration.StorageContainer);
                bool exists = container.Exists();

                if (!exists)
                    container.Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Check your blob connection string.");
                return false;
            }
            

            return true;
        }

        private static ISqlServer GetSqlServer()
        {
            foreach(ISqlServer sql in AzureHelper.AzureInstance.SqlServers.List())
            {
                if (sql.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    return sql;
                }
            }

            return null;
        }

        public static void DeployDatabase(string filePath, string connectionString, string databaseName)
        {
            Console.WriteLine("Deploying Database");

            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                BacPackage pkg = BacPackage.Load(fs);

                DacServices svc = new Microsoft.SqlServer.Dac.DacServices(connectionString);
                svc.ImportBacpac(pkg, databaseName, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        async private static Task LoadConfiguration()
        {
            Console.WriteLine($"Loading Azure Resource Group Configuration");

            //load the storage account info...
            foreach (IStorageAccount sa in AzureHelper.AzureInstance.StorageAccounts.List())
            {
                if (sa.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    Configuration.StorageAccountName = sa.Name;
                    var keys = sa.GetKeys();
                    Configuration.StorageAccountKey = keys[0].Value;
                    break;
                }
            }

            //load the search service info...
            foreach(ISearchService sa in AzureHelper.AzureInstance.SearchServices.List())
            {
                if (sa.ResourceGroupName == Configuration.ResourceGroupName)
                {
                    Configuration.SearchServiceName = sa.Name;
                    var keys = sa.GetAdminKeys();
                    Configuration.SearchKey = keys.PrimaryKey;
                    break;
                }
            }

            //load the function app
            foreach(IWebApp webApp in AzureHelper.AzureInstance.WebApps.List())
            {
                if ( webApp.ResourceGroupName == Configuration.ResourceGroupName && webApp.Inner.Kind == "functionapp")
                {
                    Configuration.FunctionAppUrl = webApp.DefaultHostName;
                }
            }

            //load the conginitive services
            var cogs = CognitiveServicesManager.Configure().Authenticate(AzureHelper.AzureCredentials, Configuration.SubscriptionId);

            CognitiveServicesAccountInner props = cogs.Inner.CognitiveServicesAccounts.GetPropertiesAsync(Configuration.ResourceGroupName, Configuration.ResourcePrefix + "-cogs").Result;
            CognitiveServicesAccountKeysInner cogkeys = cogs.Inner.CognitiveServicesAccounts.ListKeysAsync(Configuration.ResourceGroupName, Configuration.ResourcePrefix + "-cogs").Result;
            
            Configuration.CognitiveServicesResourceId = props.Id;
            Configuration.CognitiveServicesUrl = props.Endpoint;
            Configuration.CognitiveServicesKey = cogkeys.Key1;

            //maps
            AzureHelper.GetMaps();

            return;
        }

        private static void UploadZipToStorageAccount(string storageAccountName, string storageAccountKey, string storageContainer, string zipPath)
        {
            Console.WriteLine($"Uploading sample content");

            AzureStorageService svc = new AzureStorageService();

            FileInfo zip = new FileInfo(zipPath);

            //unzip it...
            Utility.ExtractZipFile(zipPath, null, zip.DirectoryName + "\\" + zip.Name.Replace(zip.Extension, ""));
            
            DirectoryInfo di = new DirectoryInfo(zip.DirectoryName + "\\" + zip.Name.Replace(zip.Extension, ""));

            if (di.Exists)
            {
                //loop all files
                foreach (FileInfo fi in di.GetFiles())
                {
                    Console.WriteLine($"Uploading file {fi.Name}");

                    //upload the file...
                    svc.UploadBlob(fi, Configuration.StorageContainer);
                }
            }
        }

        private static void SetupWebApp(IWebApp webApp, string type, Hashtable properties)
        {
            Console.WriteLine($"Setting web app properties [{webApp.Name}]");

            Dictionary<string, string> settings = new Dictionary<string, string>();

            switch (type)
            {
                case "CognitiveSearch.API":
                    settings.Add("SearchServiceName", Configuration.SearchServiceName);
                    settings.Add("SearchApiKey", Configuration.SearchKey);
                    settings.Add("SearchIndexName", "base");
                    settings.Add("SearchIndexerName", "base-indexer");
                    settings.Add("InstrumentationKey", "");
                    settings.Add("StorageAccountName", Configuration.StorageAccountName);
                    settings.Add("StorageAccountKey", Configuration.StorageAccountKey);
                    settings.Add("StorageContainerAddress", $"https://{Configuration.ResourcePrefix}storage.blob.core.windows.net/{Configuration.StorageContainer}");
                    break;
                case "CognitiveSearch.UI":
                    string functionUrl = properties["functionUrl"].ToString();
                    string apiUrl = properties["apiUrl"].ToString();

                    settings.Add("SEARCH_CONFIG_SERVICE_URL", $"https://{apiUrl}/home/getdocuments");
                    settings.Add("SUGGESTION_CONFIG_SERVICE_URL", $"https://{apiUrl}/home/getsuggestions");
                    settings.Add("FUNCTION_CONFIG_SERVICE_URL", $"https://{apiUrl}/home/getgraphdata");
                    break;
            }

            webApp.Update().WithAppSettings(settings).Apply();
        }
        private static Dictionary<string, string> GetFunctionAppSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            settings.Add("FUNCTIONS_EXTENSION_VERSION", "~2");
            settings.Add("AzureWebJobsDashboard", Configuration.BlobStorageConnectionString);
            settings.Add("AzureWebJobsStorage", Configuration.BlobStorageConnectionString);
            settings.Add("WEBSITE_CONTENTAZUREFILECONNECTIONSTRING", Configuration.BlobStorageConnectionString);
            settings.Add("WEBSITE_CONTENTSHARE", Configuration.ResourcePrefix + "-funcshare");
            settings.Add("AzureWebJobsSecretStorageType", "files");

            settings.Add("BlobStorageAccountConnectionString", Configuration.BlobStorageConnectionString);
            settings.Add("BlobContainerName", Configuration.StorageContainer);
            settings.Add("BingEndpoint", Configuration.BingEndpoint);
            settings.Add("BingKey", Configuration.BingKey);
            settings.Add("CognitiveServicesVisionUrl", Configuration.CognitiveServicesUrl + "/vision/v2.0/detect");
            settings.Add("CognitiveServicesKey", Configuration.CognitiveServicesKey);
            settings.Add("AzureMapsUrl", Configuration.AzureMapsUrl);
            settings.Add("AzureMapsKey", Configuration.AzureMapsKey);
            settings.Add("CognitiveTranslateUrl", "https://api.cognitive.microsofttranslator.com");
            settings.Add("CognitiveTranslateKey", Configuration.CognitiveServicesKey);
            settings.Add("CognitiveTranslateRegion", Configuration.Region);
            settings.Add("FormsRecognizerEndpoint", Configuration.CognitiveServicesUrl);
            settings.Add("FormsRecognizerKey", Configuration.CognitiveServicesKey);
            settings.Add("FormsModelId", "");

            return settings;
        }

        private static void SetupFunctionApp(IFunctionApp webApp, string type)
        {
            Console.WriteLine($"Setting function properties [{webApp.Name}]");

            var settings = GetFunctionAppSettings();

            webApp.Update().WithAppSettings(settings).Apply();
        }

        private static void DeployPowerBI(string name, string filePath)
        {
            
        }

        private static void DeployMLApp(string name, string filePath)
        {
            
        }

        private static IFunctionApp DeployFunctionApp(string webName, string deploymentZipPath)
        {
            Console.WriteLine($"Deploying function app [{webName}] with [{deploymentZipPath}]");

            IFunctionApp webApp = null;

            string planName = Configuration.ResourcePrefix + "-appsvc-func";
            var obj = AzureHelper.AzureInstance.AppServices.AppServicePlans.Define(planName.ToLower()).WithRegion(AzureHelper.AzureRegion).WithExistingResourceGroup(Configuration.ResourceGroupName).WithPricingTier(PricingTier.BasicB1).WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Windows);

            //deploy the webs...
            try
            {
                webName = Configuration.ResourcePrefix + "-" + webName;
                webName = webName.ToLower();

                webApp = GetFunctionApp(webName);
                IStorageAccount store = GetStorageAccount(Configuration.ResourcePrefix + "storage");

                if (webApp == null)
                {
                    var settings = GetFunctionAppSettings();
                    AzureHelper.AzureInstance.AppServices.FunctionApps.Define(webName.ToLower()).WithRegion(AzureHelper.AzureRegion).WithExistingResourceGroup(Configuration.ResourceGroupName).WithExistingStorageAccount(store).WithNewAppServicePlan(obj).WithSystemAssignedManagedServiceIdentity().WithAppSettings(settings).Create();
                }

                webApp = GetFunctionApp(webName);

                AssignApplicationInsightsKey("-insights", webApp, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (IFunctionApp)webApp;
        }

        private static IStorageAccount GetStorageAccount(string name)
        {
            foreach (IStorageAccount wa in AzureHelper.AzureInstance.StorageAccounts.List())
            {
                if (wa.ResourceGroupName == Configuration.ResourceGroupName && wa.Name == name)
                {
                    return wa;
                }
            }

            return null;
        }

        public static IWebApp GetWebApp(string webName)
        {
            foreach(IWebApp wa in AzureHelper.AzureInstance.AppServices.WebApps.List())
            {
                if ( wa.ResourceGroupName == Configuration.ResourceGroupName && wa.Name == webName)
                {
                    return wa;
                }
            }

            return null;
        }


        public static IFunctionApp GetFunctionApp(string webName)
        {
            foreach (IFunctionApp wa in AzureHelper.AzureInstance.AppServices.FunctionApps.List())
            {
                if (wa.ResourceGroupName == Configuration.ResourceGroupName && wa.Name == webName)
                {
                    return wa;
                }
            }

            return null;
        }

        private static IWebApp DeployWebApp(string webName, string deploymentZipName)
        {
            Console.WriteLine($"Deploying web app [{webName}] with [{deploymentZipName}]");

            IWebApp webApp = null;

            string planName = Configuration.ResourcePrefix + "-appsvc-web";
            var obj = AzureHelper.AzureInstance.AppServices.AppServicePlans.Define(planName.ToLower()).WithRegion(AzureHelper.AzureRegion).WithExistingResourceGroup(Configuration.ResourceGroupName).WithPricingTier(PricingTier.BasicB1).WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Windows);

            //deploy the webs...
            try
            {
                webName = Configuration.ResourcePrefix + "-" + webName;
                webName = webName.ToLower();

                webApp = GetWebApp(webName);

                if (webApp == null)
                    webApp = AzureHelper.AzureInstance.WebApps.Define(webName.ToLower()).WithRegion(AzureHelper.AzureRegion).WithExistingResourceGroup(Configuration.ResourceGroupName).WithNewWindowsPlan(obj).WithSystemAssignedManagedServiceIdentity().Create();

                AssignApplicationInsightsKey("-insights", webApp, false);

                var profile = webApp.GetPublishingProfile();
                DeployWebZip(deploymentZipName, webName, profile.GitUsername, profile.GitPassword);

                return webApp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return webApp;
        }

        public static void DeployWebZip(string appName, string webAppName, string username, string password)
        {
            try
            {
                Console.WriteLine($"Deploying Web Zip {appName}");

                var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}"));
                var file = File.ReadAllBytes(RunPath + @"\Deployment\" + appName + ".zip");
                MemoryStream stream = new MemoryStream(file);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64Auth);
                    var baseUrl = new Uri($"https://" + webAppName + ".scm.azurewebsites.net/");
                    var requestURl = baseUrl + "api/zipdeploy";
                    var httpContent = new StreamContent(stream);
                    var response = client.PostAsync(requestURl, httpContent).Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void AssignApplicationInsightsKey(string name, IWebAppBase webApp, bool noExt)
        {
            string appInsightsName = Configuration.ResourcePrefix + name;

            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);

            FileInfo fi = new FileInfo(di.Parent.FullName + @"/AzureTemplates/ApplicationInsightsKey_Assign.json");

            if (noExt)
                fi = new FileInfo(di.Parent.FullName + @"/AzureTemplates/ApplicationInsightsKey_AssignNoExt.json");

            string data = "";
            IReadOnlyDictionary<string, IAppSetting> settings = null;

            //get current access policies...
            settings = webApp.GetAppSettings();

            foreach (var p in settings)
            {
                //this is the one we are setting...
                if (p.Key != "APPINSIGHTS_INSTRUMENTATIONKEY")
                    data += $"\"{p.Value.Key}\": \"{p.Value.Value}\",";
            }
            
            Hashtable ht = new Hashtable();
            ht.Add("appInsightsName", appInsightsName.ToLower());
            ht.Add("webSiteName", webApp.Name.ToLower());
            ht.Add("existingProperties", data);

            AzureHelper.CreateDeployment(fi, "{}", ht, Configuration.ResourceGroupName);
        }

        private static string CreateApplicationInsights(string name)
        {
            Console.WriteLine("Creating App Insights");

            name = Configuration.ResourcePrefix + name;
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            FileInfo fi = new FileInfo(di.Parent.FullName + @"/AzureTemplates/ApplicationInsights_create.json");

            Hashtable ht = new Hashtable();
            ht.Add("appInsightsName", name.ToLower());
            ht.Add("location", "East US");

            AzureHelper.CreateDeployment(fi, "{}", ht, Configuration.ResourceGroupName);

            return name;
        }

        async internal void Install()
        {
            Initialize();

            await Start();

            Thread.Sleep(10000);
        }
    }
}
