using Domain.Models;
using Domain.Enums;
using PDFReader.DTOs;

namespace PDFReader.Extensions;

public static class MealScheduleExtensions
{
    public static MealScheduleDTO ToDTO(this MealSchedule schedule)
    {
        return new MealScheduleDTO
        {
            Id = schedule.Id,
            DayOfWeek = schedule.DayOfWeek switch
            {
                DaysOfWeek.Poniedzialek => "Poniedziałek",
                DaysOfWeek.Wtorek => "Wtorek",
                DaysOfWeek.Sroda => "Środa",
                DaysOfWeek.Czwartek => "Czwartek",
                DaysOfWeek.Piatek => "Piątek",
                DaysOfWeek.Sobota => "Sobota",
                DaysOfWeek.Niedziela => "Niedziela",
                _ => string.Empty
            },
            MealType = schedule.MealType switch
            {
                MealTypes.Sniadanie => "Śniadanie",
                MealTypes.DrugieSniadanie => "Drugie Śniadanie",
                MealTypes.Obiad => "Obiad",
                MealTypes.Kolacja => "Kolacja",
                _ => string.Empty
            },
            MealName = schedule.MealName,
            Time = schedule.Time,
            RecipeId = schedule.RecipeId,
        };
    }

    public static List<MealScheduleDTO> ToDTOs(this IEnumerable<MealSchedule> schedules)
    {
        return schedules.Select(s => s.ToDTO()).ToList();
    }
} 