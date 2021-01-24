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
    public class UsersController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(UserDto))]
        [Route("api/users")]
        public async Task<IHttpActionResult> GetUsers()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).ProducerId;

                if (producerId < 1)
                {
                    return NotFound();
                }

                var users = await entities.Users.Include(u => u.Producer).Select(u =>
                            new UserDto()
                            {
                                Id = u.Id,
                                Username = u.UserName,
                                Name = u.Name,
                                Surname = u.Surname,
                                DeviceId = DeviceByProducerId(producerId).Result.Id,
                                DeviceType = DeviceByProducerId(producerId).Result.DeviceType,
                                Producer = u.Producer.FirstOrDefault(p => p.Id == producerId)
                            }).Where(u => u.Producer.Id == producerId).ToListAsync();

                if (users == null)
                {
                    return NotFound();
                }

                return Ok(users);
            }
        }
        [HttpGet]
        [ResponseType(typeof(UserDto))]
        [Route("api/users/current")]
        public async Task<IHttpActionResult> GetCurrentUser()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).ProducerId;

                if (producerId < 1)
                {
                    return NotFound();
                }

                var user = await entities.Users.Include(u => u.Producer).Select(u =>
                            new UserDto()
                            {
                                Id = u.Id,
                                Username = u.UserName,
                                Name = u.Name,
                                Surname = u.Surname,
                                DeviceId = entities.Device.FirstOrDefault(d => d.ProducerId == 1).Id,
                                DeviceType = entities.Device.FirstOrDefault(d => d.ProducerId == 1).DeviceType,
                                Producer = u.Producer.FirstOrDefault(p => p.Id == producerId)
                            }).SingleOrDefaultAsync(u => u.Username == User.Identity.Name);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
        }

        private async Task<Device> DeviceByProducerId(int producerId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                Device device = await entities.Device.FirstOrDefaultAsync(d => d.ProducerId == producerId);

                if (device == null)
                {
                    return null;
                }

                return device;
            }
        }
    }
}
