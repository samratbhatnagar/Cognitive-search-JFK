using Newtonsoft.Json;

namespace KnowledgeMiningDeployer.Models
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}