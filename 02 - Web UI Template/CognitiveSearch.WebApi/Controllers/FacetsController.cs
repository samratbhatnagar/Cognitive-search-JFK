using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
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

        public FacetsController(AppInsightsConfig appInsightsConfig, SearchConfig searchConfig)
        {
            _appInsightsConfig = appInsightsConfig;
            _searchConfig = searchConfig;
            _searchClient = new SearchClient(_searchConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await Task.FromResult(new JsonResult(_searchClient.Model.Facets.Select(f => f.Name)));
        }
    }
}