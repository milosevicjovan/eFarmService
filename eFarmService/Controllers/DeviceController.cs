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
        This class provides methods for allowing our device to send and recieves neccessary data
        It will be authorized using basic authentication with encoded username and hashed password
        from [Users] table -> we don't know password, we send it both hashed and encoded
        Our device is able to post data from sensors, update settings based on water pump's state, and 
        to get settings if user changes water pump or temperature/moisture boundaries values in web app/mobile app
    */
    public class DeviceController : ApiController
    {
        [HttpPost]
        [BasicAuthentication]
        [Route("api/device/post")]
        public async Task<IHttpActionResult> Post([FromUri] DeviceData deviceData)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (deviceData == null)
            {
                return BadRequest();
            }

            deviceData.Time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                /* 
                    Because these request are sent from arduino devices, we don't have to check
                    if deviceId actually exists in table -> it must be there
                    Also there will be no null value
                */
                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                int deviceId = entities.Device.SingleOrDefault(d => d.ProducerId == producerId).Id;
                Device device = entities.Device.SingleOrDefault(d => d.Id == deviceId);

                deviceData.Device = device;
                deviceData.DeviceId = deviceId;

                if (!ModelState.IsValid)
                {
                    return BadRequest();
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
        [Route("api/device/settings")]
        public async Task<IHttpActionResult> Get()
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                int deviceId = entities.Device.SingleOrDefault(d => d.ProducerId == producerId).Id;

                var settingsForDevice = await entities.DeviceSettings.Include(d => d.Device).Select(d =>
                    new SettingsDto()
                    {
                        DeviceId = d.DeviceId,
                        MoistureMin = d.MoistureMin,
                        MoistureMax = d.MoistureMax,
                        TemperatureMin = d.TemperatureMin,
                        TemperatureMax = d.TemperatureMax,
                        WaterPump = d.WaterPump,
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
        [Route("api/device/settings")]
        public async Task<IHttpActionResult> Put([FromUri]bool waterPump)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            /* 
                Arduino just changes water pump state, we just update it to true or false
                Other settings are updated from client's side
            */

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                int deviceId = entities.Device.SingleOrDefault(d => d.ProducerId == producerId).Id;

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
