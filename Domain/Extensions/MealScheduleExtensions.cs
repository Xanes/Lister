using Domain.Models;

namespace Domain.Extensions;

public static class MealScheduleExtensions
{
    public static List<MealSchedule> RemoveDuplicates(this IEnumerable<MealSchedule>? schedules)
    {
        if (schedules == null || !schedules.Any())
            return new List<MealSchedule>();

        return schedules
            .GroupBy(s => new 
            { 
                s.DayOfWeek, 
                s.MealType, 
                s.MealName 
            })
            .Select(g => g.OrderByDescending(x => x.Time != null).First())
            .ToList();
    }
} 