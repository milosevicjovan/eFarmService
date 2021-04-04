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
    public class UsersController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(UserDto))]
        [Route("api/users")]
        public async Task<IHttpActionResult> GetUsers()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;

                if (producerId < 1)
                {
                    return NotFound();
                }

                var users = await entities.Users.Where(u => u.ProducerId == producerId).Select(u =>
                            new UserDto()
                            {
                                Id = u.Id,
                                Username = u.UserName,
                                Name = u.Name,
                                Surname = u.Surname,
                                ProducerId = u.ProducerId,
                                Producer = entities.Producer.FirstOrDefault(p => p.Id == producerId).Name
                            }).ToListAsync();

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
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;

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
                                ProducerId = u.ProducerId,
                                Producer = entities.Producer.FirstOrDefault(p => p.Id == producerId).Name
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
