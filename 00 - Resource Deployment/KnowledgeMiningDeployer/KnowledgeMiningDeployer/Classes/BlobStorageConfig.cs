using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Classes
{
    public class BlobStorageConfig
    {
        public string AccountName { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string SasToken { get; set; }
    }
}
