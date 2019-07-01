using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace KnowledgeMiningDeployer
{
    public class Configuration
    {
        static Hashtable _settings;
        public static string SubscriptionId { get; internal set; }
        public static string Region { get; internal set; }
        public static string AdminAzureClientId { get; internal set; }
        public static string AdminAzureClientSecret { get; internal set; }
        public static string TenantId { get; internal set; }
        public static string ResourceGroupName { get; internal set; }
        public static string EventHubNamespace { get; internal set; }
        public static string ResourcePrefix { get; internal set; }
        public static string SearchKey { get; internal set; }
        public static string SearchServiceName { get; internal set; }
        public static string InstrumentationKey { get; internal set; }
        public static string StorageAccountName { get; internal set; }
        public static string StorageAccountKey { get; internal set; }
        public static string StorageContainer { get; internal set; }
        public static string CognitiveServicesKey { get; internal set; }
        public static string CognitiveServicesResourceId { get; internal set; }
        public static string AzureManagementApi { get; internal set; }
        public static bool UseSampleData { get; internal set; }
        public static string CognitiveServicesUrl { get; internal set; }
        public static string SearchServiceApiVersion { get; internal set; }
        public static string FunctionAppUrl { get; internal set; }
        public static string SqlPassword { get; internal set; }
        public static string Username { get; internal set; }
        public static string Password { get; internal set; }
        public static string BlobStorageConnectionString { get; internal set; }
        public static string ConfigFilePath { get; internal set; }
        public static string CustomDataZip { get; internal set; }
        public static string BingEndpoint { get; internal set; }
        public static string BingKey { get; internal set; }
        public static bool DeployMain { get; internal set; }
        public static string AzureMapsUrl { get; internal set; }
        public static string AzureMapsKey { get; internal set; }
        public static bool DoDeployments { get; internal set; }
        public static string SearchSku { get; internal set; }
        public static int DeploymentMode { get; internal set; }

        public static Hashtable LoadConfiguration(string filename)
        {
            return Load(File.ReadAllText(filename));
        }

        public static void Load(FileInfo fi)
        {
            LoadConfiguration(fi.FullName);
        }

        private static void SetProperty(string name, string value)
        {
            PropertyInfo prop = typeof(Configuration).GetProperty(name);

            if (prop != null)
                SetProperty(prop, value);

            if (_settings.ContainsKey(name.ToLower()))
                _settings[name.ToLower()] = value;
            else
                _settings.Add(name.ToLower(), value);
        }

        private static void SetProperty(PropertyInfo pi, string inValue)
        {
            try
            {
                object value;

                switch (pi.PropertyType.FullName)
                {
                    case "System.Int32":
                        if (!string.IsNullOrEmpty(inValue))
                        {
                            value = int.Parse(inValue);
                            pi.SetValue(null, value);
                        }
                        break;
                    case "System.Decimal":
                        if (!string.IsNullOrEmpty(inValue))
                        {
                            value = decimal.Parse(inValue);
                            pi.SetValue(null, value);
                        }
                        break;
                    case "System.Guid":
                        if (!string.IsNullOrEmpty(inValue))
                        {
                            value = Guid.Parse(inValue);
                            pi.SetValue(null, value);
                        }
                        break;
                    case "System.String":
                        value = inValue;
                        pi.SetValue(null, value);
                        break;
                    case "System.Boolean":
                        pi.SetValue(null, Boolean.Parse(inValue));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public static dynamic Export(string mode, string filePath)
        {
            filePath = filePath + $"/configuration.{mode}.json";
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject(json);
        }

        public static dynamic Export(string mode)
        {
            string filePath = HostingEnvironment.MapPath($"~/");

            if (string.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath = Assembly.GetExecutingAssembly().Location);
                filePath = fi.Directory.FullName + $"\\";
            }

            return Export(mode, filePath);
        }

        public static void LoadWithMode(string mode, string filePath)
        {
            filePath = filePath + $"/configuration.{mode}.json";

            FileInfo fi = new FileInfo(filePath);

            if (!fi.Exists)
            {
                //try some manual paths...(function app)
                filePath = $"D:/home/site/wwwroot/configuration.{mode}.json";
            }

            //load configuartion
            _settings = LoadConfiguration(filePath);
            LoadConfiguration(_settings);
        }

        public static void LoadConfiguration(Hashtable ht)
        {
            PropertyInfo[] props = typeof(Configuration).GetProperties();

            foreach (string key in ht.Keys)
            {
                foreach (PropertyInfo pi in props)
                {
                    if (pi.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        SetProperty(pi, ht[key].ToString());
                    }
                }
            }
        }

        public static void LoadWithMode(string mode)
        {
            Console.WriteLine($"Loading configuration with mode: {mode}");

            string filePath = HostingEnvironment.MapPath($"~/");

            if (string.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath = Assembly.GetExecutingAssembly().Location);
                filePath = fi.Directory.FullName + $"\\";
            }

            LoadWithMode(mode, filePath);
        }

        public static Hashtable Load(string configJson)
        {
            dynamic json = JsonConvert.DeserializeObject(configJson);

            Hashtable ht = new Hashtable();

            foreach (dynamic setting in json)
            {
                ht.Add(setting.Name.ToString().ToLower(), setting.Value);
            }

            return ht;
        }
    }
}
