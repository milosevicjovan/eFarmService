using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFarmService.Models
{
    public class ProducerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string OwnerName { get; set; }
        public string OwnerSurname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

    }
}
