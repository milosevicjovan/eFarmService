using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using eFarmDataAccess;
using eFarmService.Models;
using System.Data.Entity;
using System.Web.Http.Description;
using System.Threading.Tasks;

namespace eFarmService.Controllers
{
    [Authorize]
    public class DeviceDataController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(DeviceDataDto))]
        [Route("api/device/sensors/latest")]
        //fetching latest data sent from device that current user is connected to
        public async Task<IHttpActionResult> GetData()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = entities.Users.SingleOrDefault(d => d.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1)
                {
                    return NotFound();
                }

                int maxId = entities.DeviceData.Where(d => d.DeviceId == deviceId).Max(d => (int?)d.Id) ?? 0;

                if (maxId < 1)
                {
                    return NotFound();
                }

                var deviceData = await entities.DeviceData.Include(d => d.Device)
                            .Select(d =>
                            new DeviceDataDto()
                            {
                                Id = d.Id,
                                Time = d.Time,
                                SoilMoisture = d.SoilMoisture,
                                Rain = d.Rain,
                                Water = d.Water,
                                Temperature = d.Temperature,
                                Humidity = d.Humidity,
                                Pressure = d.Pressure,
                                Altitude = d.Altitude,
                                DeviceType = d.Device.DeviceType
                            }).SingleOrDefaultAsync(d => d.Id == maxId);

                if (deviceData == null)
                {
                    return NotFound();
                }

                return Ok(deviceData);
            }
        }

        [HttpGet]
        [ResponseType(typeof(DeviceDataDto))]
        [Route("api/device/sensors/average/{date}")]
        //average for selected date
        public async Task<IHttpActionResult> GetAverageForOneDay(string date)
        {
            DateTime shortDate = Convert.ToDateTime(date);
            double soilMoistureSum, temperatureSum, humiditySum, pressureSum, altitudeSum = 0;
            int rainSum, count = 0;
            bool water = false;
            string deviceType;

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = entities.Users.SingleOrDefault(d => d.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1)
                {
                    return NotFound();
                }

                count = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                            && d.DeviceId == deviceId).CountAsync();

                if (count <= 0)
                {
                    return NotFound();
                }

                deviceType =  entities.Device.SingleOrDefault(d => d.Id == deviceId).DeviceType;

                soilMoistureSum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (double?)d.SoilMoisture) ?? 0;
                temperatureSum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (double?)d.Temperature) ?? 0;
                humiditySum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (double?)d.Humidity) ?? 0;
                pressureSum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (double?)d.Pressure) ?? 0;
                altitudeSum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (double?)d.Altitude) ?? 0;
                rainSum = await entities.DeviceData.Where(d => DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId).SumAsync(d => (int?)d.Rain) ?? 0;
                water = await entities.DeviceData.AnyAsync(d => d.Water == true && DbFunctions.TruncateTime(d.Time) == shortDate.Date
                                                            && d.DeviceId == deviceId);
            }

            double soilMoistureAvg = soilMoistureSum / count;
            double temperatureAvg = temperatureSum / count;
            double humidityAvg = humiditySum / count;
            double pressureAvg = pressureSum / count;
            double altitudeAvg = altitudeSum / count;
            double rainAvg = rainSum / count;

            DeviceDataDto data = new DeviceDataDto
            {
                Id = 0,
                Time = shortDate,
                SoilMoisture = Math.Round(soilMoistureAvg, 2),
                Temperature = Math.Round(temperatureAvg, 2),
                Humidity = Math.Round(humidityAvg, 2),
                Pressure = Math.Round(pressureAvg, 2),
                Altitude = Math.Round(altitudeAvg, 2),
                Rain = Convert.ToInt32(rainAvg),
                Water = water,
                DeviceType = deviceType
            };

            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        [HttpGet]
        [ResponseType(typeof(DeviceDataDto))]
        [Route("api/device/sensors/data")]
        //data from between two selected dates
        public async Task<IHttpActionResult> GetData(string from, string to)
        {

            DateTime fromDate = Convert.ToDateTime(from);
            DateTime toDate = Convert.ToDateTime(to);


            if (toDate < fromDate || fromDate==null || toDate==null)
            {
                return NotFound();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = entities.Users.SingleOrDefault(d => d.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1)
                {
                    return NotFound();
                }

                var deviceData = await entities.DeviceData.Where(d => d.DeviceId == deviceId &&
                                DbFunctions.TruncateTime(d.Time) >= fromDate && DbFunctions.TruncateTime(d.Time) <= toDate)
                                .Include(d => d.Device)
                                .Select(d =>
                                new DeviceDataDto()
                                {
                                    Id = d.Id,
                                    Time = d.Time,
                                    SoilMoisture = d.SoilMoisture,
                                    Rain = d.Rain,
                                    Water = d.Water,
                                    Temperature = d.Temperature,
                                    Humidity = d.Humidity,
                                    Pressure = d.Pressure,
                                    Altitude = d.Altitude,
                                    DeviceType = d.Device.DeviceType
                                }).ToListAsync();

                if (deviceData == null)
                {
                    return NotFound();
                }

                return Ok(deviceData);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/device/active/{id}")]
        public async Task<IHttpActionResult> GetDeviceActivity(int id)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = id;

                if (!entities.Device.Any(d => d.Id == deviceId))
                {
                    NotFound();
                }

                int maxId = entities.DeviceData.Where(d => d.DeviceId == deviceId).Max(d => (int?)d.Id) ?? 0;

                if (maxId < 1)
                {
                    return NotFound();
                }

                var device = await entities.DeviceData.SingleOrDefaultAsync(d => d.Id == maxId);
                DateTime lastActive = device.Time;
                DateTime now = DateTime.Now;

                System.TimeSpan diff = now.Subtract(lastActive);

                var deviceActivity = new DeviceActivityDto
                {
                    DeviceId = deviceId,
                    LastActive = lastActive
            };

                if (diff.TotalMinutes <= 2)
                {
                    deviceActivity.IsActive = true;                   
                    return Ok(deviceActivity);
                }

                deviceActivity.IsActive = false;
                return Ok(deviceActivity);

            }
        }

    }
}
