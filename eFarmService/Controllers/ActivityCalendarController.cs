using eFarmDataAccess;
using eFarmService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace eFarmService.Controllers
{
    [Authorize]
    public class ActivityCalendarController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(ActivityCalendarDTO))]
        [Route("api/calendar/activities/all")]
        public async Task<IHttpActionResult> GetAllActivities(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest("Invalid request.");
            }

            DateTime fromDate;
            DateTime toDate;

            try
            {
                fromDate = Convert.ToDateTime(from);
                toDate = Convert.ToDateTime(to);
            }
            catch (Exception)
            {
                return BadRequest("Invalid date.");
            }

            if (fromDate == null || toDate == null || toDate < fromDate)
            {
                return BadRequest("Invalid request.");
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (producerId == 0)
                {
                    return BadRequest("Not authorized!");
                }

                var activities = await entities.ActivityCalendar.Where(a => a.ProducerId == producerId &&
                                        DbFunctions.TruncateTime(a.Date) >= fromDate &&
                                        DbFunctions.TruncateTime(a.Date) <= toDate).Select(a => new ActivityCalendarDTO()
                                        {
                                            Id = a.Id,
                                            Date = (DateTime)a.Date,
                                            Description = a.Description,
                                            StatusId = (int)a.StatusId,
                                            Status = a.StatusTypes.StatusType,
                                            ProducerId = (int)a.ProducerId,
                                            Producer = a.Producer.Name,
                                            Duration = (decimal)a.Duration
                                        }).ToListAsync();

                return Ok(activities);
            }
        }

        [HttpGet]
        [ResponseType(typeof(ActivityCalendarDTO))]
        [Route("api/calendar/activities/{activityId}")]
        public async Task<IHttpActionResult> GetActivityById(int activityId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (producerId == 0)
                {
                    return BadRequest("Not authorized!");
                }

                if (activityId < 1)
                {
                    return NotFound();
                }

                var activity = await entities.ActivityCalendar.SingleOrDefaultAsync(a => a.Id == activityId);
                //activity.StatusTypes = entities.StatusTypes.SingleOrDefault(s => s.Id == activity.StatusId);
                //activity.Producer = entities.Producer.SingleOrDefault(p => p.Id == activity.ProducerId);

                if (activity.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                entities.Entry(activity).Reference(a => a.Producer).Load();
                entities.Entry(activity).Reference(a => a.StatusTypes).Load();

                if (activity == null)
                {
                    return BadRequest("Activity not found!");
                }

                ActivityCalendarDTO activityDto = new ActivityCalendarDTO()
                {
                    Id = activity.Id,
                    Date = (DateTime)activity.Date,
                    Description = activity.Description,
                    StatusId = (int)activity.StatusId,
                    Status = activity.StatusTypes.StatusType,
                    ProducerId = (int)activity.ProducerId,
                    Producer = activity.Producer.Name,
                    Duration = (decimal)activity.Duration
                };

                return Ok(activityDto);
            }
        }

        [HttpPost]
        [Route("api/calendar/activities/post")]
        public async Task<IHttpActionResult> PostActivity([FromBody]ActivityCalendar activity)
        {

            if (activity == null)
            {
                return BadRequest("Invalid JSON. Activity object is null.");
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId <= 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid JSON.");
                }

                activity.ProducerId = producerId;
                activity.Producer = entities.Producer.SingleOrDefault(p => p.Id == activity.ProducerId);
                activity.StatusTypes = entities.StatusTypes.SingleOrDefault(s => s.Id == activity.StatusId);

                if (activity.Producer == null || activity.StatusTypes == null)
                {
                    return BadRequest("Invalid JSON! Producer and/or status are incorrect!");
                }

                entities.ActivityCalendar.Add(activity);

                await entities.SaveChangesAsync();

                entities.Entry(activity).Reference(o => o.Producer).Load();
                entities.Entry(activity).Reference(o => o.StatusTypes).Load();

                var activityDto = new ActivityCalendarDTO()
                {
                    Id = activity.Id,
                    Date = (DateTime)activity.Date,
                    Description = activity.Description,
                    StatusId = (int)activity.StatusId,
                    Status = activity.StatusTypes.StatusType,
                    ProducerId = (int)activity.ProducerId,
                    Producer = activity.Producer.Name,
                    Duration = (decimal)activity.Duration
                };

                return Created("", activityDto);
            }
        }

        [HttpPut]
        [Route("api/calendar/activities/{activityId}")]
        public async Task<IHttpActionResult> UpdateOrder(int activityId, [FromBody]ActivityCalendar newActivity)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {

                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId <= 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (activityId < 1)
                {
                    return NotFound();
                }

                if (newActivity == null)
                {
                    return BadRequest("Invalid JSON! Activity object is null.");
                }

                var activity = await entities.ActivityCalendar.SingleOrDefaultAsync(a => a.Id == activityId);

                if (activity == null)
                {
                    return NotFound();
                }

                if (activity.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                activity.Date = newActivity.Date;
                activity.Description = newActivity.Description;
                activity.StatusId = newActivity.StatusId;
                activity.Duration = newActivity.Duration;
                activity.StatusTypes = entities.StatusTypes.SingleOrDefault(s => s.Id == activity.StatusId);

                await entities.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
