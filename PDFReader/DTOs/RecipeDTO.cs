using Domain.Models;

namespace PDFReader.DTOs
{
    public class RecipeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Fat { get; set; }
        public double Carbohydrates { get; set; }
        public double Fiber { get; set; }
        public List<RecipeIngredientDTO> Ingredients { get; set; } = new();
        public List<RecipeInstructionDTO> Instructions { get; set; } = new();

        public static RecipeDTO FromModel(Recipe recipe)
        {
            return new RecipeDTO
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Calories = recipe.Calories,
                Protein = recipe.Protein,
                Fat = recipe.Fat,
                Carbohydrates = recipe.Carbohydrates,
                Fiber = recipe.Fiber,
                Ingredients = recipe.Ingredients
                    .Select(i => RecipeIngredientDTO.FromModel(i))
                    .ToList(),
                Instructions = recipe.Instructions
                    .OrderBy(i => i.StepNumber)
                    .Select(i => RecipeInstructionDTO.FromModel(i))
                    .ToList()
            };
        }
    }

    public class RecipeIngredientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string WeightUnit { get; set; } = string.Empty;
        public string? MergeLog { get; set; }

        public static RecipeIngredientDTO FromModel(RecipeIngredient ingredient)
        {
            return new RecipeIngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Quantity = ingredient.Quantity,
                QuantityUnit = ingredient.QuantityUnit,
                Weight = ingredient.Weight,
                WeightUnit = ingredient.WeightUnit,
                MergeLog = ingredient.MergeLog
            };
        }
    }

    public class RecipeInstructionDTO
    {
        public int Id { get; set; }
        public int StepNumber { get; set; }
        public string Instruction { get; set; } = string.Empty;

        public static RecipeInstructionDTO FromModel(RecipeInstruction instruction)
        {
            return new RecipeInstructionDTO
            {
                Id = instruction.Id,
                StepNumber = instruction.StepNumber,
                Instruction = instruction.Instruction
            };
        }
    }
} 