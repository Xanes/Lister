namespace PDFReader.DTOs;

public class MealScheduleDTO
{
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string MealName { get; set; } = string.Empty;
    public TimeSpan? Time { get; set; }
    public int RecipeId { get; set; }
} 