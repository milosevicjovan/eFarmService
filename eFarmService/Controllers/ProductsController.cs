using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using eFarmDataAccess;
using System.Web.Http.Description;
using eFarmService.Models;
using System.Threading.Tasks;
using System.Data.Entity;

namespace eFarmService.Controllers
{
    public class ProductsController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(ProductsDto))]
        [Route("api/products/{producerId}")]
        public async Task<IHttpActionResult> Get(int producerId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (producerId < 1) 
                {
                    return NotFound();
                }

                var products = await entities.Products.Include(p => p.ProductTypes.ProductCategories).Where(p => p.ProducerId == producerId).Select(p =>
                                new ProductsDto()
                                {
                                    Id = p.Id,
                                    Category = p.ProductTypes.ProductCategories.Category,
                                    Product = p.ProductTypes.Name,
                                    Quantity = (decimal)p.Quantity,
                                    Price = (decimal)p.Price
                                }).ToListAsync();

                if (products == null)
                {
                    return NotFound();
                }
                return Ok(products);
            }
        }
    }
}
