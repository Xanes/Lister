using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class ProductsDescriptionInfo
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<string> QuantityUnits { get; set; } = new List<string>();
        public List<string> WeightUnits { get; set; } = new List<string>();
    }
} 