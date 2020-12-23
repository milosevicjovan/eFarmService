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
    public class LocationsController : ApiController
    {
        //helper method
        private bool IsActive(int id)
        {
            int deviceId = id;

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

                if (diff.TotalMinutes > 2)
                {
                    return false;
                }

                return true;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(LocationDto))]
        [Route("api/devices/locations")]
        public async Task<IHttpActionResult> Get()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                var locations = await entities.Device
                            .Select(d =>
                            new LocationDto()
                            {
                                DeviceId = d.Id,
                                DeviceType = d.DeviceType,
                                DeviceLocation = d.DeviceLocation
                            }).ToListAsync();

                foreach(LocationDto location in locations)
                {
                    if (IsActive(location.DeviceId))
                    {
                        location.IsActive = true;
                    } else
                    {
                        location.IsActive = false;
                    }
                }

                if (locations == null)
                {
                    return NotFound();
                }

                return Ok(locations);
            }
        }
    }
}
