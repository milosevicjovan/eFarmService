using System.Net;
using System.Net.Http;
using System.Web.Http;
using eFarmDataAccess;
using System.Web.Http.Description;
using eFarmService.Models;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System;
namespace eFarmService.Controllers
{
    [Authorize]
    public class LocationsController : ApiController
    {
        //helper method
        private bool IsActive(int deviceId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (!entities.Device.Any(d => d.Id == deviceId))
                {
                    return false;
                }

                int maxId = entities.DeviceData.Where(d => d.DeviceId == deviceId).Max(d => (int?)d.Id) ?? 0;

                if (maxId < 1)
                {
                    return false;
                }

                var device = entities.DeviceData.SingleOrDefault(d => d.Id == maxId);
                DateTime lastActive = device.Time;
                DateTime now = DateTime.Now;

                System.TimeSpan diff = now.Subtract(lastActive);

                if (diff.TotalMinutes <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        [HttpGet]
        [ResponseType(typeof(DeviceDto))]
        [Route("api/devices/all")]
        public async Task<IHttpActionResult> Get()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).ProducerId;

                if (producerId < 1)
                {
                    return BadRequest("Not authorized!");
                }

                var devices = await entities.Device.Where(d => d.ProducerId == 1).Select(d =>
                              new DeviceDto()
                              {
                                  DeviceId = d.Id,
                                  DeviceType = d.DeviceType,
                                  DeviceLocation = d.DeviceLocation
                              }).ToListAsync();

                if (devices == null || devices.Count <= 0)
                {
                    return NotFound();
                }

                foreach (DeviceDto device in devices)
                {
                    if (IsActive(device.DeviceId))
                    {
                        device.IsActive = true;
                    }
                    else
                    {
                        device.IsActive = false;
                    }
                }

                return Ok(devices);
            }
        }
    }
}
