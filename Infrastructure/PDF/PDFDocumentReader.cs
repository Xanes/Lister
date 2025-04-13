using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.PDF
{
    public class PDFDocumentReader
    {
        public PdfLoadedDocument Read(Stream fs)
        {
            return new PdfLoadedDocument(fs);
        }
    }
}
