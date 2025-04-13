using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ShoppingList : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; } 
        public DateTime CreatedAt { get; set; }
        public List<ProductCategoryGroup>  ProductCategories { get; set; } = new List<ProductCategoryGroup>();
        public virtual List<MealSchedule> MealSchedules { get; set; } = new();
    }
}
