using Domain.Interfaces;

namespace Domain.Models;

public record RecipeIngredient : IEntity
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public double Weight { get; set; }
    public string WeightUnit { get; set; } = string.Empty;
    public string? MergeLog { get; set; }

    // Navigation property
    public virtual Recipe? Recipe { get; set; }
} 