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
    public class IncomesController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(IncomeDTO))]
        [Route("api/incomes/all")]
        public async Task<IHttpActionResult> GetAllIncomes(string from, string to)
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

                var incomes = await entities.Incomes.Where(i => i.ProducerId == producerId &&
                                        DbFunctions.TruncateTime(i.Date) >= fromDate &&
                                        DbFunctions.TruncateTime(i.Date) <= toDate).Select(e => new IncomeDTO()
                                        {
                                            Id = e.Id,
                                            Date = (DateTime)e.Date,
                                            TypeId = (int?)e.TypeId ?? 0,
                                            TypeDescription = e.IncomeTypes.Description,
                                            Description = e.Description,
                                            Amount = (decimal?)e.Amount ?? 0,
                                            ProducerId = (int?)e.ProducerId ?? 0,
                                            Producer = e.Producer.Name
                                        }).ToListAsync();

                return Ok(incomes);
            }
        }

        [HttpGet]
        [ResponseType(typeof(IncomeDTO))]
        [Route("api/incomes/{incomeId}")]
        public async Task<IHttpActionResult> GetIncomeById(int incomeId)
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

                if (incomeId < 1)
                {
                    return NotFound();
                }

                var income = await entities.Incomes.SingleOrDefaultAsync(i => i.Id == incomeId);

                if (income.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                entities.Entry(income).Reference(i => i.Producer).Load();
                entities.Entry(income).Reference(i => i.IncomeTypes).Load();

                if (income == null)
                {
                    return BadRequest("Expense not found!");
                }

                IncomeDTO incomeDto = new IncomeDTO()
                {
                    Id = income.Id,
                    Date = (DateTime)income.Date,
                    TypeId = (int?)income.TypeId ?? 0,
                    TypeDescription = income.IncomeTypes.Description,
                    Description = income.Description,
                    Amount = (decimal?)income.Amount ?? 0,
                    ProducerId = (int?)income.ProducerId ?? 0,
                    Producer = income.Producer.Name
                };

                return Ok(incomeDto);
            }
        }

        [HttpPost]
        [Route("api/incomes/post")]
        public async Task<IHttpActionResult> PostIncome([FromBody]Incomes income)
        {

            if (income == null)
            {
                return BadRequest("Invalid JSON. Income object is null.");
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

                income.ProducerId = producerId;
                income.Producer = entities.Producer.SingleOrDefault(p => p.Id == income.ProducerId);
                income.IncomeTypes = entities.IncomeTypes.SingleOrDefault(i => i.Id == income.TypeId);

                if (income.Producer == null || income.TypeId == null)
                {
                    return BadRequest("Invalid JSON! Producer and/or type are incorrect!");
                }

                entities.Incomes.Add(income);

                await entities.SaveChangesAsync();

                entities.Entry(income).Reference(e => e.Producer).Load();
                entities.Entry(income).Reference(e => e.IncomeTypes).Load();

                var incomeDto = new IncomeDTO()
                {
                    Id = income.Id,
                    Date = (DateTime)income.Date,
                    TypeId = (int?)income.TypeId ?? 0,
                    TypeDescription = income.IncomeTypes.Description,
                    Description = income.Description,
                    Amount = (decimal?)income.Amount ?? 0,
                    ProducerId = (int?)income.ProducerId ?? 0,
                    Producer = income.Producer.Name
                };

                return Created("", incomeDto);
            }
        }

        [HttpPut]
        [Route("api/incomes/{incomeId}")]
        public async Task<IHttpActionResult> UpdateIncome(int incomeId, [FromBody]Incomes newIncome)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {

                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId <= 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (incomeId < 1)
                {
                    return NotFound();
                }

                if (newIncome == null)
                {
                    return BadRequest("Invalid JSON! Income object is null.");
                }

                var income = await entities.Incomes.SingleOrDefaultAsync(i => i.Id == incomeId);

                if (income == null)
                {
                    return NotFound();
                }

                if (income.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                income.Date = newIncome.Date;
                income.Description = newIncome.Description;
                income.Amount = newIncome.Amount;
                income.TypeId = newIncome.TypeId;
                income.IncomeTypes = entities.IncomeTypes.SingleOrDefault(i => i.Id == income.TypeId);

                await entities.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
