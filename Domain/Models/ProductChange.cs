using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ProductChange
    {
        public int Id { get; set; }
        public double? ChangedWeight { get; set; }
        public double? ChangedQuantity { get; set; }
        public bool? IsChecked { get; set; }
    }
}
