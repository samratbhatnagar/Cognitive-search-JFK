using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Classes
{
    public class SearchConfig
    {
        public string ServiceName { get; set; }
        public string Key { get; set; }
        public string DataSourceName { get; set; }
        public string IndexName { get; set; }
        public string IndexerName { get; set; }
        public string SkillsetName { get; set; }
        public string ApiVersion { get; set; }
    }
}
