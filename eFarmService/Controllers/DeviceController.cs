using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using eFarmDataAccess;
using System.Threading.Tasks;
using System.Threading;
using eFarmService.Models;
using System.Data.Entity;

namespace eFarmService.Controllers
{
    /*
        This class provides methods for allowing our device to send and recieves neccessary data.
        It will be authorized using basic authentication with encoded username and hashed password
        from [Device] table -> we don't know password, we send it both hashed and encoded
        Our device is able to upload data from sensors, update settings based on water pump's state and auto control, 
        and to get settings if user changes auto control state, or turn water pump on, 
        or changes temperature/moisture boundaries values in web app/mobile/desktop app
    */
    public class DeviceController : ApiController
    {
        [HttpPost]
        [BasicAuthentication]
        [Route("api/device/{deviceId}/post")]
        public async Task<IHttpActionResult> Post(int deviceId, [FromUri] DeviceData deviceData)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (deviceId < 1)
            {
                return BadRequest();
            }

            deviceData.Time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                /* 
                    Because these requests are sent from embedded devices, we don't have to check
                    if deviceId actually exists in table -> it must be there
                    Also there will be no null value
                */
                bool deviceExists = await entities.Device.AnyAsync(d => d.Id == deviceId);

                if (!deviceExists)
                {
                    return BadRequest("Invalid device ID!");
                }

                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                Device device = await entities.Device.SingleOrDefaultAsync(d => d.Id == deviceId);

                if (producerId != device.ProducerId)
                {
                    return BadRequest("The authorization for this request has been denied!");
                }

                deviceData.Device = device;
                deviceData.DeviceId = deviceId;

                if (deviceData == null)
                {
                    return BadRequest("Invalid data!");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model State!");
                }

                entities.DeviceData.Add(deviceData);

                await entities.SaveChangesAsync();

                entities.Entry(deviceData).Reference(d => d.Device).Load();

                var dataDto = new DeviceDataDto()
                {
                    Id = deviceData.Id,
                    Time = deviceData.Time,
                    SoilMoisture = deviceData.SoilMoisture,
                    Rain = deviceData.Rain,
                    Water = deviceData.Water,
                    Temperature = deviceData.Temperature,
                    Humidity = deviceData.Humidity,
                    Pressure = deviceData.Pressure,
                    Altitude = deviceData.Altitude,
                    DeviceType = deviceData.Device.DeviceType
                };

                return Created("", dataDto);
            }
        }

        [HttpGet]
        [BasicAuthentication]
        [Route("api/device/{deviceId}/device-settings")]
        public async Task<IHttpActionResult> Get(int deviceId)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (deviceId < 1)
            {
                return BadRequest();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                bool deviceExists = await entities.Device.AnyAsync(d => d.Id == deviceId);

                if (!deviceExists)
                {
                    return BadRequest("Invalid device ID!");
                }

                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                Device device = await entities.Device.SingleOrDefaultAsync(d => d.Id == deviceId);

                if (producerId != device.ProducerId)
                {
                    return BadRequest("The authorization for this request has been denied!");
                }

                var settingsForDevice = await entities.DeviceSettings.Include(d => d.Device).Select(d =>
                    new SettingsDto()
                    {
                        DeviceId = d.DeviceId,
                        MoistureMin = d.MoistureMin,
                        MoistureMax = d.MoistureMax,
                        TemperatureMin = d.TemperatureMin,
                        TemperatureMax = d.TemperatureMax,
                        WaterPump = d.WaterPump,
                        AutoControl = d.AutoControl,
                        DeviceType = d.Device.DeviceType
                    }).SingleOrDefaultAsync(d => d.DeviceId == deviceId);

                if (settingsForDevice == null)
                {
                    return BadRequest();
                }

                return Ok(settingsForDevice);
            }
        }

        [HttpPut]
        [BasicAuthentication]
        [Route("api/device/{deviceId}/device-settings")]
        public async Task<IHttpActionResult> Put(int deviceId, [FromUri]bool waterPump)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (deviceId < 1)
            {
                return BadRequest();
            }
            /* 
                Device just changes water pump state, we just update it to true or false
                Other settings are updated from client's side
            */

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                bool deviceExists = await entities.Device.AnyAsync(d => d.Id == deviceId);

                if (!deviceExists)
                {
                    return BadRequest("Invalid device ID!");
                }

                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                Device device = await entities.Device.SingleOrDefaultAsync(d => d.Id == deviceId);

                if (producerId != device.ProducerId)
                {
                    return BadRequest("The authorization for this request has been denied!");
                }

                var deviceSettings = await entities.DeviceSettings.SingleOrDefaultAsync(d => d.DeviceId == deviceId);

                if (deviceSettings == null)
                {
                    return BadRequest();
                }

                deviceSettings.WaterPump = waterPump;

                await entities.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
