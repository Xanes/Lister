using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Models;

public record MealSchedule : IEntity
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public DaysOfWeek DayOfWeek { get; set; }
    public MealTypes MealType { get; set; }
    public string MealName { get; set; } = string.Empty;
    public TimeSpan? Time { get; set; }
    
    // Navigation property
    public virtual ShoppingList? ShoppingList { get; set; }
} 