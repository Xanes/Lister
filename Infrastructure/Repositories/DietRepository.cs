using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Infrastructure.Repositories
{
    public class DietRepository : IDietRepository
    {
        private readonly ListerDbContext _context;

        public DietRepository(ListerDbContext listerDbContext)
        {
            _context = listerDbContext;
        }

        public async Task<ShoppingList> CreateAsync(ShoppingList entity, List<MealSchedule> mealSchedules, List<Recipe> recipes)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Handle categories
                var categories = await _context.Categories.ToListAsync();
                entity.ProductCategories?.Where(c => c.Category != null).ToList().ForEach(c =>
                {
                    var category = categories.FirstOrDefault(cat => cat.Name == c.Category?.Name);
                    if (c.Category != null && category != null)
                    {
                        c.Category = category;
                        _context.Entry(c.Category).State = EntityState.Modified;
                    }
                });

                // Add recipes first
                if (recipes?.Any() == true)
                {
                    await _context.Recipes.AddRangeAsync(recipes);
                    await _context.SaveChangesAsync();
                }

                // Add shopping list
                var result = await _context.AddAsync(entity);
                await _context.SaveChangesAsync();

                // Add meal schedules with the new shopping list ID and match recipes
                if (mealSchedules?.Any() == true)
                {
                    foreach (var schedule in mealSchedules)
                    {
                        schedule.ShoppingListId = result.Entity.Id;
                        
                        // First try exact match
                        var matchingRecipe = recipes?.FirstOrDefault(r => 
                            r.Name.Equals(schedule.MealName, StringComparison.OrdinalIgnoreCase));
                            
                        // If no exact match, try matching first 12 letters
                        if (matchingRecipe == null && schedule.MealName?.Length >= 5 && recipes?.Any() == true)
                        {
                            var scheduleName12 = schedule.MealName.Substring(0, 5).ToLower();
                            matchingRecipe = recipes.FirstOrDefault(r => 
                                r.Name?.Length >= 5 && 
                                r.Name.Substring(0, 5).ToLower() == scheduleName12);
                        }
                            
                        if (matchingRecipe != null)
                        {
                            schedule.RecipeId = matchingRecipe.Id;
                            schedule.Recipe = matchingRecipe;
                        }
                    }
                    
                    await _context.MealSchedules.AddRangeAsync(mealSchedules);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                var returnEntity = result.Entity;
                returnEntity.MealSchedules.Clear();
                return returnEntity;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ShoppingList> GetAsync(int id)
        {
            return await _context.ShoppingLists
                .Include(c => c.ProductCategories)
                .ThenInclude(c => c.Products)
                .Include(c => c.ProductCategories)
                .ThenInclude(c => c.Category)
                .FirstAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ShoppingList>> GetAllAsync()
        {
            return await _context.ShoppingLists.ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the shopping list with all related data
                var entity = await _context.ShoppingLists
                    .Include(s => s.MealSchedules)
                    .Include(s => s.ProductCategories)
                        .ThenInclude(pc => pc.Products)
                    .FirstAsync(p => p.Id == id);

                // 1. Get recipes that are used only by this shopping list
                var recipeIds = entity.MealSchedules
                    .Select(m => m.RecipeId)
                    .Distinct()
                    .ToList();

                if (recipeIds.Any())
                {
                    // Find recipes that are used ONLY by this shopping list
                    var recipesToDelete = await _context.Recipes
                        .Include(r => r.Instructions)
                        .Include(r => r.Ingredients)
                        .Where(r => recipeIds.Contains(r.Id))
                        .Where(r => !r.MealSchedules.Any(m => m.ShoppingListId != id))
                        .ToListAsync();

                    foreach (var recipe in recipesToDelete)
                    {
                        // 2. Delete recipe ingredients
                        _context.RemoveRange(recipe.Ingredients);
                        
                        // 3. Delete recipe instructions
                        _context.RemoveRange(recipe.Instructions);
                        
                        // 4. Delete the recipe itself
                        _context.Remove(recipe);
                    }
                    await _context.SaveChangesAsync();
                }

                // 5. Delete all products in all category groups
                foreach (var categoryGroup in entity.ProductCategories)
                {
                    _context.RemoveRange(categoryGroup.Products);
                }
                await _context.SaveChangesAsync();

                // 6. Delete all category groups
                _context.RemoveRange(entity.ProductCategories);
                await _context.SaveChangesAsync();

                // 7. Delete all meal schedules
                _context.RemoveRange(entity.MealSchedules);
                await _context.SaveChangesAsync();

                // 8. Finally delete the shopping list
                _context.Remove(entity);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async Task<IEnumerable<MealSchedule>> GetMealSchedulesAsync(int shoppingListId)
        {
            return await _context.MealSchedules
                .Where(m => m.ShoppingListId == shoppingListId)
                .ToListAsync();
        }

        public async Task<ShoppingList> GetListInfoAsync(int id)
        {
            var list = await _context.ShoppingLists
                .Select(l => new ShoppingList 
                { 
                    Id = l.Id, 
                    Name = l.Name, 
                    Description = l.Description, 
                    CreatedAt = l.CreatedAt 
                })
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                throw new KeyNotFoundException($"Shopping list with ID {id} not found");
            }

            return list;
        }

        public async Task<Recipe> GetRecipeAsync(int recipeId)
        {
            var recipe = await _context.Recipes.Include(r => r.Ingredients).Include(r => r.Instructions)
                .AsNoTracking()
                .Select(r => new Recipe
                {
                    Id = r.Id,
                    Name = r.Name,
                    Calories = r.Calories,
                    Protein = r.Protein,
                    Fat = r.Fat,
                    Carbohydrates = r.Carbohydrates,
                    Fiber = r.Fiber,
                    Instructions = r.Instructions
                        .OrderBy(i => i.StepNumber)
                        .Select(i => new RecipeInstruction
                        {
                            Id = i.Id,
                            StepNumber = i.StepNumber,
                            Instruction = i.Instruction
                        })
                        .ToList(),
                    Ingredients = r.Ingredients
                        .Select(i => new RecipeIngredient
                        {
                            Id = i.Id,
                            Name = i.Name,
                            Quantity = i.Quantity,
                            QuantityUnit = i.QuantityUnit,
                            Weight = i.Weight,
                            WeightUnit = i.WeightUnit,
                            MergeLog = i.MergeLog
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
            {
                throw new KeyNotFoundException($"Recipe with ID {recipeId} not found");
            }

            return recipe;
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByNameAsync(string name)
        {
            return await _context.Recipes
                .Include(r => r.Instructions)
                .Include(r => r.Ingredients)
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .ToListAsync();
        }
    }
}