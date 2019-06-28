using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Eventhub.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Maps.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer
{
    public class AzureHelper
    {
        static public AzureCredentials AzureCredentials { get; set; }
        static public IAzure AzureInstance { get; set; }
        static public Region AzureRegion { get; set; }
        static public IResourceGroup AzureResourceGroup { get; set; }
        static public IEventHubNamespace AzureEventNamespace { get; set; }

        public static void Initialize(string resourceGroupName)
        {
            Console.WriteLine($"Initializing Azure Helper");

            AzureCredentials = MakeAzureCredentials(Configuration.SubscriptionId);

            var client = RestClient
            .Configure()
            .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            .WithCredentials(AzureCredentials)
            .Build();

            //connect to azure...
            Azure.IConfigurable ic = Azure.Configure();
            Azure.IAuthenticated auth = ic.Authenticate(AzureCredentials);
            AzureInstance = auth.WithDefaultSubscription();

            //create resource group...
            AzureRegion = Region.Create(Configuration.Region);

            AzureResourceGroup = CreateResourceGroup(resourceGroupName);
        }

        static public AzureCredentials MakeAzureCredentials(string subscriptionId)
        {
            var appId = Configuration.AdminAzureClientId;
            var appSecret = Configuration.AdminAzureClientSecret;
            var tenantId = Configuration.TenantId;
            var environment = AzureEnvironment.AzureGlobalCloud;

            var credentials = new AzureCredentialsFactory()
                .FromServicePrincipal(appId, appSecret, tenantId, environment);

            return credentials.WithDefaultSubscription(Configuration.SubscriptionId.ToString());            
        }

        async public static void GetMaps()
        {
            Microsoft.Azure.Management.Maps.MapsManagementClient client = new Microsoft.Azure.Management.Maps.MapsManagementClient(AzureCredentials);
            client.SubscriptionId = Configuration.SubscriptionId;

            AzureOperationResponse<IEnumerable<MapsAccount>> res = await client.Accounts.ListByResourceGroupWithHttpMessagesAsync(Configuration.ResourceGroupName);

            foreach(MapsAccount map in res.Body)
            {
                if (map.Name == Configuration.ResourcePrefix + "-map")
                {
                    AzureOperationResponse<MapsAccountKeys> keys = await client.Accounts.ListKeysWithHttpMessagesAsync(Configuration.ResourceGroupName, map.Name);

                    Configuration.AzureMapsUrl = "https://atlas.microsoft.com";
                    Configuration.AzureMapsKey = keys.Body.PrimaryKey;
                }
            }
        }

        public static IResourceGroup CreateResourceGroup(string name)
        {
            try
            {
                if (AzureInstance == null)
                    Initialize(Configuration.ResourceGroupName);

                AzureResourceGroup = AzureInstance.ResourceGroups.Define(name).WithRegion(AzureRegion).Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return AzureResourceGroup;
        }

        private static void Initialize(object resourceGroupName)
        {
            throw new NotImplementedException();
        }

        public static IEventHubNamespace CreateEventNamespace()
        {
            return CreateEventNamespace(Configuration.EventHubNamespace);
        }

        public static IEventHubNamespace CreateEventNamespace(string name)
        {
            //create namespace...
            try
            {
                if (AzureInstance == null)
                    Initialize(Configuration.ResourceGroupName);

                IEnumerable<IEventHubNamespace> list = AzureInstance.EventHubNamespaces.List();

                foreach (IEventHubNamespace ns in list)
                {
                    if (ns.Name == name)
                    {
                        AzureEventNamespace = ns;
                        return ns;
                    }
                }

                AzureEventNamespace = AzureInstance.EventHubNamespaces.Define(name).WithRegion(AzureRegion).WithExistingResourceGroup(AzureResourceGroup.Name).Create();
                return AzureEventNamespace;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static IEventHub CreateEventHub(IEventHubNamespace ns, string eventHubName)
        {
            //create event hub
            try
            {
                if (AzureInstance == null)
                    Initialize(Configuration.ResourceGroupName);

                return AzureInstance.EventHubs.Define(eventHubName).WithExistingNamespace(ns).Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static IWebApp GetWebApp(string v)
        {
            if (AzureInstance == null)
                Initialize(Configuration.ResourceGroupName);

            IEnumerable<IWebApp> apps = AzureInstance.WebApps.List();
            IEnumerator<IWebApp> e = apps.GetEnumerator();

            while (e.MoveNext())
            {
                if (e.Current.Name.ToLower() == v.ToLower() && e.Current.ResourceGroupName == AzureResourceGroup.Name)
                    return e.Current;
            }

            return null;
        }

        public static void UploadWebAppFile(IWebAppBase webApp, byte[] file, string path)
        {
            Console.WriteLine("Uploading Configuration");

            var profile = webApp.GetPublishingProfile();
            var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{profile.GitUsername}:{profile.GitPassword}"));
            MemoryStream stream = new MemoryStream(file);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("If-Match", "*");
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64Auth);
                var baseUrl = new Uri($"https://" + webApp.Name + ".scm.azurewebsites.net/");
                var requestURl = baseUrl + path;
                var httpContent = new StreamContent(stream);
                var response = client.PutAsync(requestURl, httpContent).Result;
            }
        }

        public static bool AddEventHubIpAddress(string ipAddress)
        {
            if (AzureInstance == null)
                Initialize(Configuration.ResourceGroupName);

            return false;
        }

        public static void CreateDeployment(string jsonFile, string parametersFile, Hashtable tokens)
        {
            if (AzureInstance == null)
                Initialize(Configuration.ResourceGroupName);

            string json = "{}";
            string parameters = "{}";

            if (!string.IsNullOrEmpty(jsonFile))
                json = jsonFile;

            if (!string.IsNullOrEmpty(parametersFile))
                parameters = parametersFile;

            foreach (string key in tokens.Keys)
            {
                json = json.Replace("{" + key + "}", tokens[key].ToString());
                parameters = parameters.Replace("{" + key + "}", tokens[key].ToString());
            }

            CreateDeployment(json, parameters);
        }

        public static void CreateDeployment(FileInfo jsonFile, FileInfo paramatersFile, Hashtable tokens, string resourceGroupName)
        {
            if (AzureInstance == null)
                Initialize(resourceGroupName);

            string json = "{}";
            string parameters = "{}";

            if (jsonFile != null)
                json = File.ReadAllText(jsonFile.FullName);

            if (paramatersFile != null)
                parameters = File.ReadAllText(paramatersFile.FullName);

            CreateDeployment(json, parameters, tokens);
        }

        public static void CreateDeployment(FileInfo jsonFile, string parameters, Hashtable tokens, string resourceGroupName)
        {
            if (AzureInstance == null)
                Initialize(resourceGroupName);

            string json = "{}";
            
            if (jsonFile != null)
                json = File.ReadAllText(jsonFile.FullName);

            CreateDeployment(json, parameters, tokens);
        }

        public static void CreateDeployment(string templateJson, string parameters)
        {
            if (AzureInstance == null)
                Initialize(Configuration.ResourceGroupName);

            if (string.IsNullOrEmpty(parameters))
                parameters = "{}";

            Guid id = Guid.NewGuid();

            try
            {
                IDeployment deploy = AzureInstance.Deployments.Define(id.ToString())
                        .WithExistingResourceGroup(AzureResourceGroup.Name)
                        .WithTemplate(templateJson)
                        .WithParameters(parameters)
                        .WithMode(DeploymentMode.Incremental) //do not change this!!!
                        .Create();

                //Console.WriteLine(deploy.ProvisioningState);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
