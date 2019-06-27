using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Models
{
    public class BlobContext
    {
        public FileInfo FileInfo { get; set; }
        public byte[] Data { get; set; }
        public string ContainerName { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
