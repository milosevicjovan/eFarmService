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
        [Route("api/device/current/settings")]
        public async Task<IHttpActionResult> GetSettings()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = entities.Users.SingleOrDefault(d => d.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1)
                {
                    return NotFound();
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
        [Route("api/device/current/settings")]
        public async Task<IHttpActionResult> UpdateSettings([FromBody]DeviceSettings newSettings)
        {

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int deviceId = entities.Users.SingleOrDefault(d => d.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1 || newSettings == null)
                {
                    return NotFound();
                }

                var deviceSettings = await entities.DeviceSettings.SingleOrDefaultAsync(d => d.DeviceId == deviceId);

                if (deviceSettings == null)
                {
                    return BadRequest();
                }

                deviceSettings.MoistureMin = newSettings.MoistureMin;
                deviceSettings.MoistureMax = newSettings.MoistureMax;
                deviceSettings.TemperatureMax = newSettings.TemperatureMax;
                deviceSettings.TemperatureMin = newSettings.TemperatureMin;
                deviceSettings.WaterPump = newSettings.WaterPump;

                await entities.SaveChangesAsync();

                return Ok();

            }
        }

    }
}
