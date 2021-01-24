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
using System.Threading;

namespace eFarmService.Controllers
{
    public class ProductsController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(ProductDto))]
        [Route("api/producers/products/{producerId}")]
        public async Task<IHttpActionResult> Get(int producerId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (producerId < 1) 
                {
                    return NotFound();
                }

                var products = await entities.Products.Include(p => p.ProductTypes.ProductCategories).Where(p => p.ProducerId == producerId).Select(p =>
                                new ProductDto()
                                {
                                    Id = p.Id,
                                    Category = p.ProductTypes.ProductCategories.Category,
                                    Product = p.ProductTypes.Name,
                                    Quantity = (decimal)p.Quantity,
                                    Price = (decimal)p.Price,
                                    Producer = p.Producer.Name
                                }).ToListAsync();

                if (products == null)
                {
                    return NotFound();
                }
                return Ok(products);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(ProductDto))]
        [Route("api/products/{productId}")]
        public async Task<IHttpActionResult> GetProductById(int productId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (productId < 1)
                {
                    return NotFound();
                }

                var products = await entities.Products.Include(p => p.ProductTypes.ProductCategories).Where(p => p.Id == productId).Select(p =>
                                new ProductDto()
                                {
                                    Id = p.Id,
                                    Category = p.ProductTypes.ProductCategories.Category,
                                    Product = p.ProductTypes.Name,
                                    Quantity = (decimal)p.Quantity,
                                    Price = (decimal)p.Price,
                                    Producer = p.Producer.Name
                                }).SingleOrDefaultAsync();

                if (products == null)
                {
                    return NotFound();
                }
                return Ok(products);
            }
        }

        [HttpPut]
        [Route("api/products/update/{productId}")]
        public async Task<IHttpActionResult> UpdateProduct(int productId, [FromBody]Products newProduct) {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                if (productId < 1)
                {
                    return NotFound();
                }

                if (newProduct == null)
                {
                    return BadRequest();
                }

                var product = await entities.Products.SingleOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound();
                }

                product.ProductTypeId = newProduct.ProductTypeId;
                product.Quantity = newProduct.Quantity;
                product.Price = newProduct.Price;

                await entities.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPost]
        [Route("api/products/post")]
        public async Task<IHttpActionResult> Post([FromBody]Products product)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (product == null)
            {
                return BadRequest();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(d => d.UserName == username).ProducerId;
                Producer producer = entities.Producer.SingleOrDefault(p => p.Id == producerId);
                product.Producer = producer;
                product.ProducerId = producerId;
                product.ProductTypes = entities.ProductTypes.SingleOrDefault(p => p.Id == product.ProductTypeId);

                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                entities.Products.Add(product);

                await entities.SaveChangesAsync();

                entities.Entry(product).Reference(p => p.Producer).Load();
                entities.Entry(product).Reference(p => p.ProductTypes).Load();

                var productDto = new ProductDto()
                {
                    Id = product.Id,
                    Category = product.ProductTypes.ProductCategories.Category,
                    Product = product.ProductTypes.Name,
                    Quantity = (decimal)product.Quantity,
                    Price = (decimal)product.Price,
                    Producer = product.Producer.Name
                };

                return Created("", productDto);
            }
        }
    }
}
