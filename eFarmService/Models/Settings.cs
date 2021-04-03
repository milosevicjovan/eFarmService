using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class SettingsDto
    {
            public int DeviceId { get; set; }
            public Nullable<int> MoistureMin { get; set; }
            public Nullable<int> MoistureMax { get; set; }
            public Nullable<int> TemperatureMin { get; set; }
            public Nullable<int> TemperatureMax { get; set; }
            public Nullable<bool> WaterPump { get; set; }
            public Nullable<bool> AutoControl { get; set; }

            public string DeviceType { get; set; }
    }
}