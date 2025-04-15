using Domain.Interfaces;

namespace Domain.Models;

public record RecipeInstruction : IEntity
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;

    // Navigation property
    public virtual Recipe? Recipe { get; set; }
} 