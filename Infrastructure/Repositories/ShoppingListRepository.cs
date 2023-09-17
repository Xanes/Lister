using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Infrastructure.Repositories
{
    public class ShoppingListRepository : IRepository<ShoppingList>
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
            return await _context.ShoppingLists.Include(c => c.ProductCategories).ThenInclude(c => c.Products).FirstAsync(p => p.Id == id);
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
    }
}