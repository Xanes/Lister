using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietMergerLib.Models
{
    public class ProductCategoryGroup
    {
        public ProductCategoryGroup(string name, List<Product> products)
        {
            Name = name;
            Products = products;
        }

        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }
}
