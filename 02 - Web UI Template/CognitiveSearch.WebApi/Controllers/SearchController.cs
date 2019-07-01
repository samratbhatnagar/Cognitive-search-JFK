using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.Azure.Search;
using CognitiveSearch.Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchConfig _searchConfig;
        private readonly BlobStorageConfig _storageConfig;
        private readonly SearchClient _searchClient;

        public SearchController(SearchConfig searchConfig, BlobStorageConfig storageConfig)
        {
            _searchConfig = searchConfig;
            _searchClient = new SearchClient(_searchConfig);
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody]JObject data, [FromQuery]SearchFacet[] searchFacets = null, [FromQuery]int currentPage = 1)
        {
            var token = await BlobStorageClient.GetContainerSasUriAsync(_storageConfig);
            //var selectFilter = _searchClient.

            return new JsonResult("");
        }
    }
}