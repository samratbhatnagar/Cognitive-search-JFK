namespace CognitiveSearch.Web.Configuration
{
    public class ApiConfig
    {
        public string Protocol { get; set; }
        public string BaseUrl { get; set; }
        public string Url => $"{Protocol}://{BaseUrl}";
    }
}