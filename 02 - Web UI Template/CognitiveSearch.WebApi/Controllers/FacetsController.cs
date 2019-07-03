using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FacetsController : ControllerBase
    {
        private readonly AppInsightsConfig _appInsightsConfig;
        private readonly SearchConfig _searchConfig;
        private readonly SearchClient _searchClient;
        private readonly TelemetryClient _telemetryClient;

        public FacetsController(AppInsightsConfig appInsightsConfig, SearchConfig searchConfig, TelemetryClient telemetryClient)
        {
            _appInsightsConfig = appInsightsConfig;
            _searchConfig = searchConfig;
            _telemetryClient = telemetryClient;
            _telemetryClient.InstrumentationKey = _appInsightsConfig.InstrumentationKey;
            _searchClient = new SearchClient(_searchConfig, _telemetryClient);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await Task.FromResult(new JsonResult(_searchClient.Model.Facets.Select(f => f.Name)));
        }
    }
}