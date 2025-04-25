using Domain.Models;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IScheduleFinder<T>
    {
        /// <summary>
        /// Extracts the meal schedule from a PDF document page containing the schedule.
        /// </summary>
        /// <param name="pdfLoadedDocument">The loaded PDF document.</param>
        /// <param name="shoppingListId">The ID of the associated shopping list.</param>
        /// <returns>A list of meal schedule entries.</returns>
        List<MealSchedule> GetMealsSchedule(T pdfLoadedDocument, int shoppingListId);
    }
} 