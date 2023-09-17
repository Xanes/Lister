using Domain.Models;
using Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public class ListerDbContext : DbContext
    {
        public ListerDbContext(DbContextOptions<ListerDbContext> contextOptions) : base(contextOptions)
        {
        }

        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ProductCategoryGroup> ProductCategoryGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public void OnModelCreation(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        }
    }
}