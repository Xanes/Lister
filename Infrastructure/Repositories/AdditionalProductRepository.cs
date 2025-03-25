using Domain.Interfaces;
using Domain.Models;
using Domain.Exceptions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AdditionalProductRepository : IAdditionalProductRepository
    {
        private readonly ListerDbContext _context;
        private readonly IReadOnlyRepository<ShoppingList> _shoppingListRepository;

        public AdditionalProductRepository(
            ListerDbContext context,
            IReadOnlyRepository<ShoppingList> shoppingListRepository)
        {
            _context = context;
            _shoppingListRepository = shoppingListRepository;
        }

        public async Task<ShoppingList> AddProductsToCategoriesAsync(int shoppingListId, IEnumerable<(int categoryId, List<Product> products)> categoryProducts)
        {
            // Get the shopping list
            var shoppingList = await _shoppingListRepository.GetAsync(shoppingListId);
            if (shoppingList == null)
            {
                throw new KeyNotFoundException($"Shopping list with ID {shoppingListId} not found");
            }

            // Process each category and its products
            foreach (var (categoryId, products) in categoryProducts)
            {
                // Validate that all products have IsTemporary flag set to true
                foreach (var product in products)
                {
                    if (!product.IsTemporary)
                    {
                        throw new DomainValidationException($"Product '{product.Name}' must have IsTemporary set to true");
                    }
                }

                // Find the category in the shopping list
                var category = shoppingList.ProductCategories
                    .FirstOrDefault(pc => pc.Category?.Id == categoryId);

                if (category == null)
                {
                    // Find the category in the database
                    var categoryEntity = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == categoryId);

                    if (categoryEntity == null)
                    {
                        // Skip if category doesn't exist
                        continue;
                    }

                    // Create new product category group
                    category = new ProductCategoryGroup
                    {
                        Category = categoryEntity,
                        Products = new List<Product>()
                    };

                    shoppingList.ProductCategories.Add(category);
                }

                // Add products to the category
                foreach (var product in products)
                {
                    category.Products.Add(product);
                }
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Return updated shopping list
            return shoppingList;
        }
    }
} 