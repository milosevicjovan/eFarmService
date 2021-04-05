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
    public class ProducersController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(ProducerDTO))]
        [AllowAnonymous]
        [Route("api/producers/all")]
        public async Task<IHttpActionResult> GetProducers()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                var producers = await entities.Producer.Select(p => new ProducerDTO()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Location = p.Location,
                    Description = p.Description,
                    OwnerName = entities.Users.FirstOrDefault(u => u.Id.Equals(p.OwnerId)).Name,
                    OwnerSurname = entities.Users.FirstOrDefault(u => u.Id.Equals(p.OwnerId)).Surname,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber
                }).ToListAsync();

                if (producers == null || producers.Count < 1)
                {
                    return NotFound();
                }

                return Ok(producers);
            }
        }
        
        [HttpGet]
        [ResponseType(typeof(ProducerDTO))]
        [AllowAnonymous]
        [Route("api/producers/{producerId}")]
        public async Task<IHttpActionResult> GetProducerById(int producerId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (producerId < 1)
                {
                    return BadRequest();
                }

                bool producerExists = await entities.Producer.AnyAsync(p => p.Id == producerId);

                if (!producerExists)
                {
                    return BadRequest("Invalid producer ID!");
                }

                Producer producer = await entities.Producer.SingleOrDefaultAsync(p => p.Id == producerId);

                ProducerDTO producerDto = new ProducerDTO()
                {
                    Id = producer.Id,
                    Name = producer.Name,
                    Location = producer.Location,
                    Description = producer.Description,
                    OwnerName = entities.Users.FirstOrDefault(u => u.Id.Equals(producer.OwnerId)).Name,
                    OwnerSurname = entities.Users.FirstOrDefault(u => u.Id.Equals(producer.OwnerId)).Surname,
                    Email = producer.Email,
                    PhoneNumber = producer.PhoneNumber
                };

                if (producerDto == null)
                {
                    return NotFound();
                }

                return Ok(producerDto);
            }
        }

        [HttpPost]
        [Route("api/producers/post")]
        public async Task<IHttpActionResult> PostProducer([FromBody]Producer producer)
        {
            if (producer == null)
            {
                return BadRequest();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid JSON.");
                }

                producer.OwnerId = userId;

                entities.Producer.Add(producer);

                await entities.SaveChangesAsync();

                ProducerDTO producerDto = new ProducerDTO()
                {
                    Id = producer.Id,
                    Name = producer.Name,
                    Location = producer.Location,
                    Description = producer.Description,
                    OwnerName = entities.Users.FirstOrDefault(u => u.Id.Equals(producer.OwnerId)).Name,
                    OwnerSurname = entities.Users.FirstOrDefault(u => u.Id.Equals(producer.OwnerId)).Surname,
                    Email = producer.Email,
                    PhoneNumber = producer.PhoneNumber
                };

                return Created("", producerDto);
            }
        }

        [HttpPut]
        [Route("api/producers/{producerId}/update")]
        public async Task<IHttpActionResult> UpdateProducer(int producerId, [FromBody]Producer newProducer)
        {
            if (newProducer == null)
            {
                return BadRequest();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                bool producerExists = await entities.Producer.AnyAsync(p => p.Id == producerId);

                if (!producerExists)
                {
                    return BadRequest("Invalid producer ID!");
                }

                var producer = await entities.Producer.SingleOrDefaultAsync(p => p.Id == producerId);

                if (!producer.OwnerId.Equals(userId))
                {
                    return BadRequest("Only producer's owner is authorized to update information!");
                }

                producer.Name = newProducer.Name;
                producer.Location = newProducer.Location;
                producer.Description = newProducer.Description;
                producer.PhoneNumber = newProducer.PhoneNumber;
                producer.Email = newProducer.Email;
                producer.OwnerId = userId;

                await entities.SaveChangesAsync();

                return Ok("Successfully updated producer with ID=" + producerId);
            }
        }
    }
}
