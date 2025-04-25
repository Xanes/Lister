using Domain.Interfaces;
using Syncfusion.Pdf.Parsing;

namespace Infrastructure.PDF
{
    public class PDFDocumentReader : IPDFDocumentReader<PdfLoadedDocument>
    {
        public PdfLoadedDocument Read(Stream fs)
        {
            return new PdfLoadedDocument(fs);
        }
    }
}