using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProductsDescriptionInfoRepository : IReadOnlyRepository<ProductsDescriptionInfo>
    {
        private readonly ListerDbContext _context;
        private readonly IDietRepository _shoppingListRepository;

        public ProductsDescriptionInfoRepository(
            ListerDbContext context, 
            IDietRepository shoppingListRepository)
        {
            _context = context;
            _shoppingListRepository = shoppingListRepository;
        }

        public async Task<ProductsDescriptionInfo> GetAsync(int shoppingListId)
        {
            var shoppingList = await _shoppingListRepository.GetAsync(shoppingListId);
            
            if (shoppingList == null)
            {
                throw new KeyNotFoundException($"Shopping list with ID {shoppingListId} not found");
            }

            var result = new ProductsDescriptionInfo();
            
            // Get all categories from the database
            result.Categories = await _context.Categories.ToListAsync();
            
            // Get distinct quantity units from products in the shopping list
            var products = shoppingList.ProductCategories.SelectMany(c => c.Products).ToList();
            result.QuantityUnits = products
                .Where(p => !string.IsNullOrEmpty(p.QuantityUnit))
                .Select(p => p.QuantityUnit)
                .Distinct()
                .OfType<string>()
                .ToList();
            
            // Get distinct weight units from products in the shopping list
            result.WeightUnits = products
                .Where(p => !string.IsNullOrEmpty(p.WeightUnit))
                .Select(p => p.WeightUnit)
                .Distinct()
                .OfType<string>()
                .ToList();
            
            return result;
        }
    }
} 