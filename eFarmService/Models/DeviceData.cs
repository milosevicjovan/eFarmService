using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eFarmService.Models
{
    public class DeviceDataDto
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public Nullable<double> SoilMoisture { get; set; }
        public Nullable<int> Rain { get; set; }
        public Nullable<bool> Water { get; set; }
        public Nullable<double> Temperature { get; set; }
        public Nullable<double> Humidity { get; set; }
        public Nullable<double> Pressure { get; set; }
        public Nullable<double> Altitude { get; set; }
        public string DeviceType { get; set; }
    }
}