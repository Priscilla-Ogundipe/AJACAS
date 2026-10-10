using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using System.Text;

namespace AJACAS.Services
{
    public class ResumeTextExtractor
    {
        public string ExtractText(string filePath, string extension)
        {
            extension = extension.ToLowerInvariant();

            return extension switch
            {
                ".pdf" => ExtractPdfText(filePath),

                ".docx" => ExtractDocxText(filePath),

                ".doc" => "DOC format extraction is not currently supported.",

                _ => string.Empty
            };
        }


        private string ExtractPdfText(string filePath)
        {
            var builder = new StringBuilder();

            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    builder.AppendLine(page.Text);
                }
            }

            return builder.ToString();
        }


        private string ExtractDocxText(string filePath)
        {
            var builder = new StringBuilder();

            using (var document =
                   WordprocessingDocument.Open(filePath, false))
            {
                var body = document.MainDocumentPart?
                    .Document?
                    .Body;

                if (body != null)
                {
                    builder.AppendLine(body.InnerText);
                }
            }

            return builder.ToString();
        }
    }
}