using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class DeviceActivityDto
    {
        public int DeviceId { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsActive { get; set; }
    }
}