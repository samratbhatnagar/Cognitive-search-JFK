using KnowledgeMiningDeployer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Helpers
{

    public class AzureStorageService
    {
        string _subscriptionId;
        string _resourceGroupName;
        string _storageAccountName;
        string _storageAccountKey;

        string _url;
        string _key;

        public AzureStorageService()
        {
            this._storageAccountName = Configuration.StorageAccountName;
            this._storageAccountKey = Configuration.StorageAccountKey;
            this._subscriptionId = Configuration.SubscriptionId;
            this._resourceGroupName = Configuration.ResourceGroupName;
        }

        public AzureStorageService(string subscriptionId, string resourceGroupName, string storageAccountName, string storageAccountKey)
        {
            this._subscriptionId = subscriptionId;
            this._resourceGroupName = resourceGroupName;
            this._storageAccountKey = storageAccountKey;
            this._storageAccountName = storageAccountName;
        }

        public string Url
        {
            get
            {
                return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", _storageAccountName, _storageAccountKey);
            }
            set
            {
                _url = value;
            }
        }

        string _token;

        public string Key
        {
            get
            {
                return Configuration.StorageAccountKey;
            }
            set
            {
                _key = value;
            }
        }

        internal async Task<dynamic> GetStorageKeysAsync(string token)
        {
            var uri = new Uri($"{Configuration.AzureManagementApi}/subscriptions/{_subscriptionId}/resourceGroups/{_resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{_storageAccountName}/listKeys?api-version=2016-01-01");
            var content = new StringContent(string.Empty, Encoding.UTF8, "text/html");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.PostAsync(uri, content))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var keys = JsonConvert.DeserializeObject(responseText);
                    return keys;
                }
            }
        }

        public object StartLogBlob(Guid applicationId)
        {
            CloudStorageAccount strAcc = GetStorageAccount();
            CloudBlobClient blobClient = strAcc.CreateCloudBlobClient();

            //Setup our container we are going to use and create it.
            CloudBlobContainer container = blobClient.GetContainerReference("importlogs");
            container.CreateIfNotExistsAsync();

            // Build my typical log file name.
            DateTime date = DateTime.Today;
            DateTime dateLogEntry = DateTime.Now;
            // This creates a reference to the append blob we are going to use.
            CloudAppendBlob appBlob = container.GetAppendBlobReference(
                string.Format("{0}{1}", date.ToString("yyyyMMdd"), ".log"));

            // Now we are going to check if todays file exists and if it doesn't we create it.
            if (!appBlob.Exists())
            {
                appBlob.CreateOrReplace();
            }

            return appBlob;
        }

        public CloudStorageAccount GetStorageAccount()
        {
            CloudStorageAccount storageAccount;
            string storageConnectionString = Url;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                return storageAccount;

            return null;
        }

        public string UploadBlob(BlobContext ctx)
        {
            CloudStorageAccount storageAccount = GetStorageAccount();

            try
            {
                if (storageAccount != null)
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer cloudBlobContainer = null;

                    try
                    {
                        // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
                        cloudBlobContainer = cloudBlobClient.GetContainerReference(ctx.ContainerName);

                        if (!cloudBlobContainer.Exists())
                            cloudBlobContainer.Create();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Container creation failed: {ex.Message}");
                    }

                    // Set the permissions so the blobs are public. 
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };

                    cloudBlobContainer.SetPermissions(permissions);

                    byte[] data = ctx.Data;
                    string name = ctx.Name;

                    if (ctx.FileInfo != null)
                    {
                        name = ctx.FileInfo.Name;
                        data = File.ReadAllBytes(ctx.FileInfo.FullName);

                        if (string.IsNullOrEmpty(ctx.Name))
                            ctx.Name = ctx.FileInfo.Name;
                    }

                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(name);

                    //azure will append if we don't delete first...
                    if (cloudBlockBlob.Exists())
                        cloudBlockBlob.Delete();

                    cloudBlockBlob.UploadFromByteArray(data, 0, data.Length);
                    return cloudBlockBlob.Uri.ToString();
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Blob upload failed: {ex.Message}");
            }

            return null;
        }

        public string UploadBlob(FileInfo fi, string containerName)
        {
            BlobContext ctx = new BlobContext();
            ctx.FileInfo = fi;
            ctx.ContainerName = containerName;
            ctx.Type = "General";
            return UploadBlob(ctx);
        }

        public string UploadBlob(FileInfo fi)
        {
            BlobContext ctx = new BlobContext();
            ctx.FileInfo = fi;
            ctx.Type = "General";
            return UploadBlob(ctx);
        }

        public string UploadBlob(string containerName, byte[] fileBytes, string name)
        {
            BlobContext ctx = new BlobContext();
            ctx.Data = fileBytes;
            ctx.Name = name;
            ctx.Type = containerName;
            return UploadBlob(ctx);
        }

        public string UploadBlob(string name, byte[] data)
        {
            BlobContext ctx = new BlobContext();
            ctx.Data = data;
            ctx.Name = name;
            ctx.Type = "General";
            return UploadBlob(ctx);
        }
    }
}