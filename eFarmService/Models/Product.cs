using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFarmService.Models
{
    class ProductDTO
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string Producer { get; set; }
    }
}
