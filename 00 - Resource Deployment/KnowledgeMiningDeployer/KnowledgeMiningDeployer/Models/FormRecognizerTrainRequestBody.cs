using Newtonsoft.Json;

namespace KnowledgeMiningDeployer.Models
{
    public class FormRecognizerTrainRequestBody
    {
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}