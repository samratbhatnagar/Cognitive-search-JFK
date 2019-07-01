using CognitiveSearch.Azure.Search;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionsController : ControllerBase
    {
        private readonly SearchConfig _searchConfig;
        private readonly SearchClient _searchClient;

        public SuggestionsController(SearchConfig searchConfig)
        {
            _searchConfig = searchConfig;
            _searchClient = new SearchClient(_searchConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string search = "")
        {
            if(string.IsNullOrWhiteSpace(search))
            {
                return new JsonResult("");
            }

            search = search.Replace("-", "").Replace("?", "");

            var response =  await _searchClient.Suggest(search, false);
            var list = response.Results.Select(r => r.Text);
            return await Task.FromResult(new JsonResult(list));
        }
    }
}