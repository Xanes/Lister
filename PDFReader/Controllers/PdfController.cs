using DietMergerLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PDFReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        [HttpPost]
        [Route(nameof(ReadPdf))]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public IActionResult ReadPdf(IFormFile file)
        {
            PDFProductsFinder finder = new PDFProductsFinder();
            return Ok(finder.FindProducts(file.OpenReadStream()));
        }
    }
}
