using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Diet
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public List<MealSchedule> MealSchedules { get; set; } = new List<MealSchedule>();
        public List<ProductCategoryGroup> ShoppingList { get; set; } = new List<ProductCategoryGroup>();
    }
}
