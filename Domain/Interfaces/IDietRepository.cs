using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDietRepository
    {
        Task<ShoppingList> CreateAsync(ShoppingList entity, List<MealSchedule> mealSchedules);
        Task DeleteAsync(int id);
        Task<IEnumerable<ShoppingList>> GetAllAsync();
        Task<ShoppingList> GetAsync(int id);
        Task ResetAsync(int shoppingListId);
        Task UpdateAsync(List<ProductChange> productChanges);
        Task<IEnumerable<MealSchedule>> GetMealSchedulesAsync(int shoppingListId);
        Task<ShoppingList> GetListInfoAsync(int id);
    }
}
