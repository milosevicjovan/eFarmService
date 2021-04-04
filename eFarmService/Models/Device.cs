using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class DeviceDto
    {
        public int DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceLocation { get; set; }
        public bool IsActive { get; set; }
    }
}