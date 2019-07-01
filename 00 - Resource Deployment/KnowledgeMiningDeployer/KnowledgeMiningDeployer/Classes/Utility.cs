using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace KnowledgeMiningDeployer.Classes
{
    public class Utility
    {
        static public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            if (!File.Exists(archiveFilenameIn))
                return;

            ZipFile zf = null;

            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);

                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        static public String ParseValue(String line, String startToken, String endToken)
        {
            if (startToken == null)
            {
                return "";
            }

            try
            {
                if (startToken == "")
                {
                    return line.Substring(0, line.IndexOf(endToken));
                }
                else
                {

                    String rtn = line.Substring(line.IndexOf(startToken));

                    if (endToken == "")
                        return line.Substring(line.IndexOf(startToken) + startToken.Length);
                    else
                        return rtn.Substring(startToken.Length, rtn.IndexOf(endToken, startToken.Length) - startToken.Length).Replace("\n", "").Replace("\t", "");
                }
            }
            catch (Exception)
            {
                
            }
            finally
            {
            }

            return "";
        }

        public static void CheckAzureDataSource_CosmosDb(dynamic ds)
        {
            //TODO
        }

        public static bool CheckAzureDataSource_StorageAccount_Blob(dynamic ds)
        {
            try
            {
                //Check the Blob storage connection string...
                CloudStorageAccount storageAccount;
                CloudStorageAccount.TryParse(ds.ConnectionString, out storageAccount);
                CloudBlobClient c = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = c.GetContainerReference(ds.ContainerName);
                bool exists = container.Exists();

                if (!exists)
                    container.Create();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth Error: Check your blob connection string {Configuration.BlobStorageConnectionString}.");
                return false;
            }
        }

        public static void CheckAzureDataSource_StorageAccount_Table(dynamic ds)
        {
            //TODO
        }

        public static void CheckAzureDataSource_AzureSql(dynamic ds)
        {
            //TODO
        }

    }
}
