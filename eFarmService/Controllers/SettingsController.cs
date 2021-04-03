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
    public class SettingsController : ApiController
    {
        [HttpGet]
        [Route("api/device/{deviceId}/settings")]
        public async Task<IHttpActionResult> GetSettings(int deviceId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).ProducerId;

                if (producerId < 1 || deviceId < 1)
                {
                    return NotFound();
                }

                bool deviceExists = entities.Device.Any(d => d.Id == deviceId && d.ProducerId == producerId);

                if (!deviceExists)
                {
                    return BadRequest("Invalid request!");
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
                         DeviceType = d.Device.DeviceType,
                         AutoControl = d.AutoControl
                     }).SingleOrDefaultAsync(d => d.DeviceId == deviceId);

                if (settingsForDevice == null)
                {
                    return BadRequest();
                }

                return Ok(settingsForDevice);
            }
        }

        [HttpPut]
        [Route("api/device/{deviceId}/settings")]
        public async Task<IHttpActionResult> UpdateSettings(int deviceId, [FromBody]DeviceSettings newSettings)
        {

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).ProducerId;

                if (producerId < 1 || deviceId < 1 || newSettings == null)
                {
                    return NotFound();
                }

                bool deviceExists = entities.Device.Any(d => d.Id == deviceId && d.ProducerId == producerId);

                if (!deviceExists)
                {
                    return BadRequest("Invalid request!");
                }

                var deviceSettings = await entities.DeviceSettings.SingleOrDefaultAsync(d => d.DeviceId == deviceId);

                if (deviceSettings == null)
                {
                    return BadRequest();
                }

                if (newSettings.MoistureMin != null)
                {
                    deviceSettings.MoistureMin = newSettings.MoistureMin;
                }
                if (newSettings.MoistureMax != null) {
                    deviceSettings.MoistureMax = newSettings.MoistureMax;
                }
                if (newSettings.TemperatureMax != null)
                {
                    deviceSettings.TemperatureMax = newSettings.TemperatureMax;
                }
                if (newSettings.TemperatureMin != null)
                {
                    deviceSettings.TemperatureMin = newSettings.TemperatureMin;
                }
                if (newSettings.WaterPump != null)
                {
                    deviceSettings.WaterPump = newSettings.WaterPump;
                }
                if (newSettings.AutoControl != null)
                {
                    deviceSettings.AutoControl = newSettings.AutoControl;
                } 

                await entities.SaveChangesAsync();

                return Ok();

            }
        }

    }
}
