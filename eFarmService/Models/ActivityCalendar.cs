using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class ActivityCalendarDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public int ProducerId { get; set; }
        public string Producer { get; set; }
        public decimal Duration { get; set; }
    }
}