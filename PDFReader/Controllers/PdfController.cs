using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.PDF;
using Domain.Models;
using Domain.Interfaces;
using Domain.Exceptions;
using PDFReader.DTOs;
using PDFReader.Extensions;

namespace PDFReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ISettings _settings;
        private readonly IRepository<ShoppingList, ProductChange> _shoppingListRepository;
        private readonly IReadOnlyRepository<ProductsDescriptionInfo> _productsDescriptionInfoRepository;
        private readonly IAdditionalProductRepository _additionalProductRepository;

        public PdfController(
            ISettings settings, 
            IRepository<ShoppingList, ProductChange> shoppingListRepository,
            IReadOnlyRepository<ProductsDescriptionInfo> productsDescriptionInfoRepository,
            IAdditionalProductRepository additionalProductRepository) 
        {
            _settings = settings;
            _shoppingListRepository = shoppingListRepository;
            _productsDescriptionInfoRepository = productsDescriptionInfoRepository;
            _additionalProductRepository = additionalProductRepository;
        }

        [HttpPost]
        [Route(nameof(ReadPdf))]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ReadPdf(string name, string description, List<IFormFile> fileList)
        {
            PDFProductsFinder finder = new PDFProductsFinder(_settings);
            var shoppingLists = fileList.Select(file => finder.FindProducts(file.OpenReadStream())).ToList();

            ShoppingListMerger merger = new ShoppingListMerger();

            var content = shoppingLists.First();

            for(int i = 1; i < shoppingLists.Count(); i++) 
            {
                content = merger.MergeShoppingLists(content, shoppingLists[i]);
            }

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
        [Route(nameof(GetProductsDescriptionInfo))]
        public async Task<IActionResult> GetProductsDescriptionInfo(int shoppingListId)
        {
            try
            {
                var result = await _productsDescriptionInfoRepository.GetAsync(shoppingListId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route(nameof(AddAdditionalProducts))]
        public async Task<IActionResult> AddAdditionalProducts(AdditionalProductRequest request)
        {
            try
            {
                // Validate request
                if (request.CategoryProducts == null || !request.CategoryProducts.Any())
                {
                    return BadRequest("At least one category with products is required");
                }

                // Validate each product
                foreach (var categoryProducts in request.CategoryProducts)
                {
                    if (categoryProducts.CategoryId <= 0)
                    {
                        return BadRequest("Invalid category ID");
                    }

                    if (categoryProducts.Products == null || !categoryProducts.Products.Any())
                    {
                        return BadRequest("Each category must have at least one product");
                    }

                    foreach (var product in categoryProducts.Products)
                    {
                        if (!product.IsValid())
                        {
                            return BadRequest($"Invalid product data for {product.Name}");
                        }
                    }
                }

                // Convert DTOs to domain models and add products to categories
                var result = await _additionalProductRepository.AddProductsToCategoriesAsync(
                    request.ShoppingListId,
                    request.CategoryProducts.ToDomainAdditionalProducts()
                );
                    
                return Ok(result);
            }
            catch (Domain.Exceptions.DomainValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpPatch]
        [Route(nameof(UpdateProducts))]
        public async Task<IActionResult> UpdateProducts(List<ProductChange> productChanges)
        {
            await _shoppingListRepository.UpdateAsync(productChanges);
            return Ok();
        }

        [HttpPatch]
        [Route(nameof(ResetList))]
        public async Task<IActionResult> ResetList(int shoppingListId)
        {
            await _shoppingListRepository.ResetAsync(shoppingListId);
            return Ok();
        }
    }
}
