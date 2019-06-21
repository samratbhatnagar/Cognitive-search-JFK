namespace CognitiveSearchInitializer.Configuration
{
    public class AppConfig
    {
        public BlobStorageConfig BlobStorage { get; set; }
        public CognitiveServicesConfig CognitiveServices { get; set; }
        public FunctionAppConfig FunctionApp { get; set; }
        public SearchConfig Search { get; set; }
    }
}