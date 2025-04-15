using Domain.Interfaces;

namespace Domain.Models;

public record Recipe : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbohydrates { get; set; }
    public double Fiber { get; set; }

    // Navigation properties
    public virtual ICollection<RecipeInstruction> Instructions { get; set; } = new List<RecipeInstruction>();
    public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public virtual ICollection<MealSchedule> MealSchedules { get; set; } = new List<MealSchedule>();
} 