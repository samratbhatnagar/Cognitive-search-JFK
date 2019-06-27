using Newtonsoft.Json;

namespace KnowledgeMiningDeployer.Models
{
    public class Error
    {
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}