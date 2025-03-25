using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Infrastructure.Repositories
{
    public class ShoppingListRepository : IRepository<ShoppingList, ProductChange>
    {
        private readonly ListerDbContext _context;

        public ShoppingListRepository(ListerDbContext listerDbContext)
        {
            _context = listerDbContext;
        }

        public async Task<ShoppingList> CreateAsync(ShoppingList entity)
        {
            var categories = await _context.Categories.ToListAsync();
            entity.ProductCategories?.Where(c => c.Category != null ).ToList().ForEach(c =>
            {
                var category = categories.FirstOrDefault(cat => cat.Name == c.Category?.Name);
                if (c.Category != null && category != null)
                {
                    c.Category = category;
                    _context.Entry(c.Category).State = EntityState.Modified;
                }
                
            });
            var result = await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<ShoppingList> GetAsync(int id)
        {
            return await _context.ShoppingLists.Include(c => c.ProductCategories).ThenInclude(c => c.Products).Include(c => c.ProductCategories).ThenInclude(c => c.Category).FirstAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ShoppingList>> GetAllAsync()
        {
            return await _context.ShoppingLists.ToListAsync();
        }

        public async Task DeleteAsync(int id)
        { 
            var entity = await _context.ShoppingLists.Include(c => c.ProductCategories).ThenInclude(c => c.Products).FirstAsync(p => p.Id == id);
            entity.ProductCategories.SelectMany(p => p.Products).ToList().ForEach(p => _context.Remove(p));
            entity.ProductCategories.ForEach(c => _context.Remove(c));
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(List<ProductChange> productChanges)
        {
            foreach (var productChange in productChanges)
            {
                var product = _context.Products.First(p => p.Id == productChange.Id);
                product.ChangedWeight = productChange.ChangedWeight;
                product.ChangedQuantity = productChange.ChangedQuantity;
                product.IsChecked = productChange.IsChecked ?? false;
                _context.Update(product);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetAsync(int shoppingListId)
        {
            ShoppingList list = await GetAsync(shoppingListId);
            var allProducts = list.ProductCategories.SelectMany(p => p.Products).ToList();
            
            // Identify temporary products to be removed
            var temporaryProducts = allProducts.Where(p => p.IsTemporary).ToList();
            
            // Identify regular products to be reset
            var regularProducts = allProducts.Where(p => !p.IsTemporary).ToList();
            
            // Reset regular products
            foreach (var product in regularProducts)
            {
                product.ChangedWeight = null;
                product.ChangedQuantity = null;
                product.IsChecked = false;
            }
            
            // Remove temporary products
            foreach (var product in temporaryProducts)
            {
                _context.Products.Remove(product);
            }
            
            // Update the regular products
            _context.UpdateRange(regularProducts);
            
            await _context.SaveChangesAsync();
        }
    }
}