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
    public class ExpensesController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(ExpenseDTO))]
        [Route("api/expenses/all")]
        public async Task<IHttpActionResult> GetAllExpenses(string from, string to)
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

                var expenses = await entities.Expenses.Where(e => e.ProducerId == producerId &&
                                        DbFunctions.TruncateTime(e.Date) >= fromDate &&
                                        DbFunctions.TruncateTime(e.Date) <= toDate).Select(e => new ExpenseDTO()
                                        {
                                            Id = e.Id,
                                            Date = (DateTime)e.Date,
                                            TypeId = (int?)e.TypeId ?? 0,
                                            TypeDescription = e.ExpenseTypes.Description,
                                            Description = e.Description,
                                            Amount = (decimal?)e.Amount ?? 0,
                                            ProducerId = (int?)e.ProducerId ?? 0,
                                            Producer = e.Producer.Name
                                        }).ToListAsync();

                return Ok(expenses);
            }
        }

        [HttpGet]
        [ResponseType(typeof(ExpenseDTO))]
        [Route("api/expenses/{expenseId}")]
        public async Task<IHttpActionResult> GetExpenseById(int expenseId)
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

                if (expenseId < 1)
                {
                    return NotFound();
                }

                var expense = await entities.Expenses.SingleOrDefaultAsync(e => e.Id == expenseId);

                if (expense.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                entities.Entry(expense).Reference(e => e.Producer).Load();
                entities.Entry(expense).Reference(e => e.ExpenseTypes).Load();

                if (expense == null)
                {
                    return BadRequest("Expense not found!");
                }

                ExpenseDTO expenseDto = new ExpenseDTO()
                {
                    Id = expense.Id,
                    Date = (DateTime)expense.Date,
                    TypeId = (int?)expense.TypeId ?? 0,
                    TypeDescription = expense.ExpenseTypes.Description,
                    Description = expense.Description,
                    Amount = (decimal?)expense.Amount ?? 0,
                    ProducerId = (int?)expense.ProducerId ?? 0,
                    Producer = expense.Producer.Name
                };

                return Ok(expenseDto);
            }
        }

        [HttpPost]
        [Route("api/expenses/post")]
        public async Task<IHttpActionResult> PostExpense([FromBody]Expenses expense)
        {

            if (expense == null)
            {
                return BadRequest("Invalid JSON. Expense object is null.");
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

                expense.ProducerId = producerId;
                expense.Producer = entities.Producer.SingleOrDefault(p => p.Id == expense.ProducerId);
                expense.ExpenseTypes = entities.ExpenseTypes.SingleOrDefault(e => e.Id == expense.TypeId);

                if (expense.Producer == null || expense.TypeId == null)
                {
                    return BadRequest("Invalid JSON! Producer and/or type are incorrect!");
                }

                entities.Expenses.Add(expense);

                await entities.SaveChangesAsync();

                entities.Entry(expense).Reference(e => e.Producer).Load();
                entities.Entry(expense).Reference(e => e.ExpenseTypes).Load();

                var expenseDto = new ExpenseDTO()
                {
                    Id = expense.Id,
                    Date = (DateTime)expense.Date,
                    TypeId = (int?)expense.TypeId ?? 0,
                    TypeDescription = expense.ExpenseTypes.Description,
                    Description = expense.Description,
                    Amount = (decimal?)expense.Amount ?? 0,
                    ProducerId = (int?)expense.ProducerId ?? 0,
                    Producer = expense.Producer.Name
                };

                return Created("", expenseDto);
            }
        }

        [HttpPut]
        [Route("api/expenses/{expenseId}")]
        public async Task<IHttpActionResult> UpdateExpense(int expenseId, [FromBody]Expenses newExpense)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {

                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId <= 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (expenseId < 1)
                {
                    return NotFound();
                }

                if (newExpense == null)
                {
                    return BadRequest("Invalid JSON! Expense object is null.");
                }

                var expense = await entities.Expenses.SingleOrDefaultAsync(e => e.Id == expenseId);

                if (expense == null)
                {
                    return NotFound();
                }

                if (expense.ProducerId != producerId)
                {
                    return BadRequest("Not authorized!");
                }

                expense.Date = newExpense.Date;
                expense.Description = newExpense.Description;
                expense.Amount = newExpense.Amount;
                expense.TypeId = newExpense.TypeId;
                expense.ExpenseTypes = entities.ExpenseTypes.SingleOrDefault(e => e.Id == expense.TypeId);

                await entities.SaveChangesAsync();
                return Ok();
            }
        }
    }
}
