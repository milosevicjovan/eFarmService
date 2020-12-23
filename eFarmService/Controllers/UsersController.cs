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
                int deviceId = entities.Users.SingleOrDefault(u => u.UserName == User.Identity.Name).DeviceId;

                if (deviceId < 1)
                {
                    return NotFound();
                }

                var users = await entities.Users.Include(u => u.Device).Select(u =>
                            new UserDto()
                            {
                                Id = u.Id,
                                Username = u.UserName,
                                Name = u.Name,
                                Surname = u.Surname,
                                DeviceId = u.DeviceId,
                                DeviceType = u.Device.DeviceType
                            }).Where(u => u.DeviceId == deviceId).ToListAsync();

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

                var user = await entities.Users.Include(u => u.Device).Select(u =>
                            new UserDto()
                            {
                                Id = u.Id,
                                Username = u.UserName,
                                Name = u.Name,
                                Surname = u.Surname,
                                DeviceId = u.DeviceId,
                                DeviceType = u.Device.DeviceType
                            }).SingleOrDefaultAsync(u => u.Username == User.Identity.Name);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
        }
    }
}
