using Newtonsoft.Json;

namespace CognitiveSearchInitializer.Models
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}