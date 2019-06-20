namespace CognitiveSearchInitializer.Configuration
{
    public class AppConfig
    {
        public BlobStorageConfig BlobStorage { get; set; }
        public CognitiveServicesConfig CognitiveServices { get; set; }
        public CosmosDbConfig CosmosDb { get; set; }
        public FunctionAppConfig FunctionApp { get; set; }
        public PersonalizerConfig Personalizer { get; set; }
        public SearchConfig Search { get; set; }
    }
}