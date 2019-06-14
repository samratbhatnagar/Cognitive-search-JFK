using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JfkWebApiSkills.HeaderExtractor
{
    public class HeaderExtractor
    {
        public static List<string> GetHeadersForDocument(string httpLocation, string contentType)
        {
            using (WebClient webConnection = new WebClient())
            {

                using (var mStream = new MemoryStream(webConnection.DownloadData(httpLocation)))
                {
                    if (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                    {
                        using (var doc = WordprocessingDocument.Open(mStream, false))
                        {
                            var paragraphs = doc.MainDocumentPart.Document.Body
                             .OfType<Paragraph>()
                             .Where(p => p.ParagraphProperties != null &&
                                         p.ParagraphProperties.ParagraphStyleId != null &&
                                         p.ParagraphProperties.ParagraphStyleId.Val.Value.Contains("Heading")).ToList();

                            //var allStyles = DocumentFormat.OpenXml.Wordprocessing.
                            return paragraphs.Select(a => a.InnerText).ToList();
                        }
                    }
                    return null;
                }
            }
        }
    }
}
