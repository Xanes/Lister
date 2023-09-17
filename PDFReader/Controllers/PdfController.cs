using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.PDF;
using Domain.Models;
using Domain.Interfaces;

namespace PDFReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ISettings _settings;
        private readonly IRepository<ShoppingList> _shoppingListRepository;
        public PdfController(ISettings settings, IRepository<ShoppingList> shoppingListRepository) 
        {
            _settings = settings;
            _shoppingListRepository= shoppingListRepository;
        }
        [HttpPost]
        [Route(nameof(ReadPdf))]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ReadPdf(string name, string description, IFormFile file)
        {
            PDFProductsFinder finder = new PDFProductsFinder(_settings);
            var content = finder.FindProducts(file.OpenReadStream());
            ShoppingList list = new ShoppingList()
            {
                ProductCategories = content,
                Name = name,
                Description = description,
                CreatedAt= DateTime.UtcNow
            };

            var result = await _shoppingListRepository.CreateAsync(list);

            return Ok(result);
        }

        [HttpGet]
        [Route(nameof(GetList))]
        public async Task<IActionResult> GetList(int Id)
        {
            return Ok(await _shoppingListRepository.GetAsync(Id));
        }

        [HttpGet]
        [Route(nameof(GetAllLists))]
        public async Task<IActionResult> GetAllLists()
        {
            return Ok(await _shoppingListRepository.GetAllAsync());
        }

        [HttpDelete]
        [Route(nameof(DeleteList))]
        public async Task<IActionResult> DeleteList(int id)
        {
            await _shoppingListRepository.DeleteAsync(id);
            return Ok();
        }
    }
}
