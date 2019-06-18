using Microsoft.CognitiveSearch.Skills.Hocr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JfkWebApiSkills.HeaderExtractor
{
    public class OcrHeaderExtractor
    {
        public static List<string> GetHeaders(List<OcrLayoutText> ocrData)
        {
            if (!ocrData.Any())
                return new List<string>();
            var heights = ocrData.SelectMany(x => x.Lines.Select(line => new
            {
                line.Text,
                height = Math.Abs(line.BoundingBox.OrderBy(a => a.Y).ToArray()[2].Y - line.BoundingBox.OrderBy(a => a.Y).ToArray()[1].Y),
                lineLength = line.Text.Length
            }));
            var groupedHeights = heights.GroupBy(a => a.height, (b, c) => new { height = b, count = c.Count(), maxLength = c.Max(d => d.lineLength) }).OrderBy(a => a.height);
            //var commonSize = groupedHeights.OrderByDescending(a => a.count).Take(6);
            //var headerHeight = groupedHeights.Where(a => a.height > 25 && a.maxLength <= 50).Min(a => a.height);
            return heights.Where(a => a.height > 25 && a.lineLength <= 50).Select(a => a.Text).ToList();
        }
    }
}
