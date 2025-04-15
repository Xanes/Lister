using Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.PDF;
using Domain.Models;
using Domain.Interfaces;
using Domain.Exceptions;
using PDFReader.DTOs;
using PDFReader.Extensions;
using Domain.Extensions;

namespace PDFReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly ISettings _settings;
        private readonly IDietRepository _dietRepository;
        private readonly IReadOnlyRepository<ProductsDescriptionInfo> _productsDescriptionInfoRepository;
        private readonly IAdditionalProductRepository _additionalProductRepository;

        public PdfController(
            ISettings settings, 
            IDietRepository dietRepository,
            IReadOnlyRepository<ProductsDescriptionInfo> productsDescriptionInfoRepository,
            IAdditionalProductRepository additionalProductRepository) 
        {
            _settings = settings;
            _dietRepository = dietRepository;
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
            ScheduleFinder scheduleFinder = new ScheduleFinder(_settings);
            PDFDocumentReader reader = new PDFDocumentReader();
            ReceipeFinder receipeFinder = new ReceipeFinder(_settings);

            var documents = fileList.Select(f => reader.Read(f.OpenReadStream())).ToList();
            var shoppingLists = documents.Select(file => finder.FindProducts(file)).ToList();
            var mealScheduleItems = documents.Select(d => scheduleFinder.GetMealsSchedule(d, 0)).ToList();
            var recipes = documents.SelectMany(d => receipeFinder.GetReceipes(d)).ToList().MergeDuplicateRecipes();

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

            var result = await _dietRepository.CreateAsync(list, mealScheduleItems.SelectMany(t => t).RemoveDuplicates(), recipes);

            return Ok(result);
        }

        [HttpGet]
        [Route(nameof(GetList))]
        public async Task<IActionResult> GetList(int Id)
        {
            return Ok(await _dietRepository.GetAsync(Id));
        }

        [HttpGet]
        [Route(nameof(GetMealSchedule))]
        public async Task<IActionResult> GetMealSchedule(int shoppingListId)
        {
            try
            {
                var mealSchedules = await _dietRepository.GetMealSchedulesAsync(shoppingListId);
                return Ok(mealSchedules.ToDTOs());
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
            return Ok(await _dietRepository.GetAllAsync());
        }

        [HttpGet]
        [Route(nameof(GetListInfo))]
        public async Task<IActionResult> GetListInfo(int id)
        {
            try
            {
                var list = await _dietRepository.GetListInfoAsync(id);
                return Ok(list);
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

        [HttpDelete]
        [Route(nameof(DeleteList))]
        public async Task<IActionResult> DeleteList(int id)
        {
            await _dietRepository.DeleteAsync(id);
            return Ok();
        }

        [HttpPatch]
        [Route(nameof(UpdateProducts))]
        public async Task<IActionResult> UpdateProducts(List<ProductChange> productChanges)
        {
            await _dietRepository.UpdateAsync(productChanges);
            return Ok();
        }

        [HttpPatch]
        [Route(nameof(ResetList))]
        public async Task<IActionResult> ResetList(int shoppingListId)
        {
            await _dietRepository.ResetAsync(shoppingListId);
            return Ok();
        }

        [HttpGet]
        [Route(nameof(GetRecipe))]
        public async Task<IActionResult> GetRecipe(int recipeId)
        {
            try
            {
                var recipe = await _dietRepository.GetRecipeAsync(recipeId);
                var recipeDto = RecipeDTO.FromModel(recipe);
                return Ok(recipeDto);
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
    }
}
