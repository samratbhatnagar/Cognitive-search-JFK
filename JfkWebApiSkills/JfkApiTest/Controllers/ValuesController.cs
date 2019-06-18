using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveSearch.Skills.Hocr;
using OpenXmlPowerTools;

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
            using (var fStream = System.IO.File.Open(@"c:\temp\DVTk RIS Emulator User Manual.docx", FileMode.Open))
            {
                using (var doc = WordprocessingDocument.Open(fStream, false))
                {
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        PageTitle = "My Page Title"
                    };
                    XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                    // Note: the XHTML returned by ConvertToHtmlTransform contains objects of type
                    // XEntity. PtOpenXmlUtil.cs defines the XEntity class. See
                    // http://blogs.msdn.com/ericwhite/archive/2010/01/21/writing-entity-references-using-linq-to-xml.aspx
                    // for detailed explanation.
                    //
                    // If you further transform the XML tree returned by ConvertToHtmlTransform, you
                    // must do it correctly, or entities do not serialize properly.

                    System.IO.File.WriteAllText(@"c:\temp\Test.html", html.ToStringNewLineOnAttributes());
                }
            }
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
            var response = new Microsoft.CognitiveSearch.WebApiSkills.WebApiSkillResponse();
            //var content2 = await new StreamReader(Request.Body).ReadToEndAsync();
            foreach (var recordRequest in content.Values)
            {
                response.Values.Add(new Microsoft.CognitiveSearch.WebApiSkills.WebApiResponseRecord { RecordId = recordRequest.RecordId });
                var data = recordRequest.Data["ocrData"].ToString();
                var ocrData = Newtonsoft.Json.JsonConvert.DeserializeObject<OcrLayoutText>(data);
                
                System.IO.File.WriteAllText(string.Format(@"C:\temp\searchContent{0}.txt", DateTime.UtcNow.ToFileTime()), data);
            }
            //using (WebClient webConnection = new WebClient())
            //{
            //    foreach (var recordRequest in content.Values)
            //    {
            //        var data = recordRequest.Data["fullDocument"].ToString();
            //        var fullDocument = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            //        var location = fullDocument["metadata_storage_path"] as string;
            //        var sasToken = fullDocument["metadata_storage_sas_token"] as string;
            //        var responseDict = new Dictionary<string, object>();

            //        using (var mStream = new MemoryStream(await webConnection.DownloadDataTaskAsync(location + sasToken)))
            //        {
            //            using (var doc = WordprocessingDocument.Open(mStream, false))
            //            {
            //                var paragraphs = doc.MainDocumentPart.Document.Body
            //                 .OfType<Paragraph>()
            //                 .Where(p => p.ParagraphProperties != null &&
            //                             p.ParagraphProperties.ParagraphStyleId != null &&
            //                             p.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading")).ToList();

            //                //var allStyles = DocumentFormat.OpenXml.Wordprocessing.
            //                responseDict.Add("headings", paragraphs.Select(a=>a.InnerText).ToList());
            //            }
            //        }
            //        response.Values.Add(new Microsoft.CognitiveSearch.WebApiSkills.WebApiResponseRecord { Data = responseDict, RecordId = recordRequest.RecordId });
            //    }
            //}
            //System.IO.File.WriteAllText(string.Format(@"C:\temp\headingContent{0}.txt", DateTime.UtcNow.ToFileTime()), Newtonsoft.Json.JsonConvert.SerializeObject(response));
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
