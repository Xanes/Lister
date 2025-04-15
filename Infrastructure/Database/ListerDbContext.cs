using Domain.Models;
using Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<PasswordConfig> PasswordConfigs { get; set; }
        public DbSet<MealSchedule> MealSchedules { get; set; }
        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Then create MealSchedule with the foreign key
            modelBuilder.ApplyConfiguration(new MealScheduleConfiguration());
            
            // Apply remaining configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListerDbContext).Assembly);
        }
    }
}