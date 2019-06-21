namespace CognitiveSearchInitializer.Configuration
{
    public class BlobStorageConfig
    {
        public string AccountName { get; set; }
        public string ConnectionString { get; set; }
        public string SourceContainerName { get; set; }
        public string ExtractionContainerName { get; set; }
        public string SasToken { get; set; }

    }
}