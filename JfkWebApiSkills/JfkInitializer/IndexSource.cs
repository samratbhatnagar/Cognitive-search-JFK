using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JfkInitializer
{
    public class IndexSource
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public DataSourceType Type { get; set; }
        [JsonProperty("credentials")]
        public string Credentials { get; set; }
        [JsonProperty("container")]
        public string Container { get; set; }
        [JsonProperty("inputMapping")]
        public FieldMapping[] InputMapping { get; set; }
        [JsonProperty("outputMapping")]
        public FieldMapping[] OutputMapping { get; set; }

    }
}
