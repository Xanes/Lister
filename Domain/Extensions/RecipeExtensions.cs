using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Extensions
{
    public static class RecipeExtensions
    {
        public static List<Recipe> MergeDuplicateRecipes(this List<Recipe> recipes)
        {
            if (recipes == null || !recipes.Any())
                return new List<Recipe>();

            // Group recipes by name
            var groupedRecipes = recipes.GroupBy(r => r.Name);
            var mergedRecipes = new List<Recipe>();

            foreach (var group in groupedRecipes)
            {
                // If there's only one recipe with this name, add it directly
                if (group.Count() == 1)
                {
                    mergedRecipes.Add(group.First());
                    continue;
                }

                // Merge multiple recipes with the same name
                var baseRecipe = group.First();
                var mergedRecipe = new Recipe
                {
                    Name = baseRecipe.Name,
                    Calories = baseRecipe.Calories,
                    Protein = baseRecipe.Protein,
                    Fat = baseRecipe.Fat,
                    Carbohydrates = baseRecipe.Carbohydrates,
                    Fiber = baseRecipe.Fiber,
                    Instructions = new List<RecipeInstruction>(baseRecipe.Instructions)
                };

                // Group all ingredients from all recipes with the same name
                var allIngredients = group.SelectMany(r => r.Ingredients).ToList();
                var groupedIngredients = allIngredients.GroupBy(i => i.Name);
                
                // Process each unique ingredient
                foreach (var ingredientGroup in groupedIngredients)
                {
                    var ingredientName = ingredientGroup.Key;
                    var ingredients = ingredientGroup.ToList();
                    
                    // If there's only one instance of this ingredient, add it directly
                    if (ingredients.Count == 1)
                    {
                        mergedRecipe.Ingredients.Add(ingredients.First());
                        continue;
                    }
                    
                    // Merge ingredients
                    var baseIngredient = ingredients.First();
                    var mergedQuantity = ingredients.Sum(i => i.Quantity);
                    var mergedWeight = ingredients.Sum(i => i.Weight);
                    
                    // Create merged ingredient
                    var mergedIngredient = new RecipeIngredient
                    {
                        Name = baseIngredient.Name,
                        Quantity = mergedQuantity,
                        QuantityUnit = baseIngredient.QuantityUnit,
                        Weight = mergedWeight,
                        WeightUnit = baseIngredient.WeightUnit,
                        RecipeId = mergedRecipe.Id
                    };
                    
                    // Create merge log showing how values were combined
                    var weightLog = string.Join(" + ", ingredients.Select(i => i.Weight));
                    mergedIngredient.MergeLog = $"{weightLog}";
                    
                    mergedRecipe.Ingredients.Add(mergedIngredient);
                }
                
                mergedRecipes.Add(mergedRecipe);
            }
            
            return mergedRecipes;
        }
    }
} 