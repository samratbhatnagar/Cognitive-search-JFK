using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeMiningDeployer.Classes
{
    public class AppConfig
    {
        public AnomalyDetectorConfig AnomalyDetector { get; set; }
        public BlobStorageConfig BlobStorage { get; set; }
        public CognitiveServicesConfig CognitiveServices { get; set; }
        public CosmosDbConfig CosmosDb { get; set; }
        public FormRecognizerConfig FormRecognizer { get; set; }
        public FunctionAppConfig FunctionApp { get; set; }
        public PersonalizerConfig Personalizer { get; set; }
        public SearchConfig Search { get; set; }
    }
}
