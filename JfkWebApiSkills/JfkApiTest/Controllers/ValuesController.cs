using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;

namespace JfkApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task<Microsoft.CognitiveSearch.WebApiSkills.WebApiSkillResponse> Post(Microsoft.CognitiveSearch.WebApiSkills.WebApiSkillRequest content)
        {
            //var content = await new StreamReader(Request.Body).ReadToEndAsync();
            //System.IO.File.WriteAllText(string.Format(@"C:\temp\searchContent{0}.txt", DateTime.UtcNow.ToFileTime()), content);
            var response = new Microsoft.CognitiveSearch.WebApiSkills.WebApiSkillResponse();
            using (WebClient webConnection = new WebClient())
            {
                foreach (var recordRequest in content.Values)
                {
                    var data = recordRequest.Data["fullDocument"].ToString();
                    var fullDocument = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    var location = fullDocument["metadata_storage_path"] as string;
                    var sasToken = fullDocument["metadata_storage_sas_token"] as string;
                    var responseDict = new Dictionary<string, object>();

                    using (var mStream = new MemoryStream(await webConnection.DownloadDataTaskAsync(location + sasToken)))
                    {
                        using (var doc = WordprocessingDocument.Open(mStream, false))
                        {
                            var paragraphs = doc.MainDocumentPart.Document.Body
                             .OfType<Paragraph>()
                             .Where(p => p.ParagraphProperties != null &&
                                         p.ParagraphProperties.ParagraphStyleId != null &&
                                         p.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading")).ToList();

                            //var allStyles = DocumentFormat.OpenXml.Wordprocessing.
                            responseDict.Add("headings", paragraphs.Select(a=>a.InnerText).ToList());
                        }
                    }
                    response.Values.Add(new Microsoft.CognitiveSearch.WebApiSkills.WebApiResponseRecord { Data = responseDict, RecordId = recordRequest.RecordId });
                }
            }
            System.IO.File.WriteAllText(string.Format(@"C:\temp\headingContent{0}.txt", DateTime.UtcNow.ToFileTime()), Newtonsoft.Json.JsonConvert.SerializeObject(response));
            return response;
        }
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
