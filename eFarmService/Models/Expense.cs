using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFarmService.Models
{
    public class ExpenseDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TypeId { get; set; }
        public string TypeDescription { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int ProducerId { get; set; }
        public string Producer { get; set; }
    }
}
