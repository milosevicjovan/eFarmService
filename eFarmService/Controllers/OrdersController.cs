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
    public class OrdersController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(OrderDTO))]
        [Route("api/orders/{orderId}")]
        public async Task<IHttpActionResult> GetOrderById(int orderId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (orderId < 1)
                {
                    return NotFound();
                }

                var order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId && o.UserId.Equals(userId));

                if (producerId > 0)
                {
                    order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId && o.ProducerId == producerId);
                }

                if (order == null)
                {
                    return BadRequest("Order not found!");
                }

                OrderDTO orderDto = new OrderDTO()
                {
                    Id = order.Id,
                    Time = order.Time,
                    CustomerName = order.CustomerName,
                    CustomerAddress = order.CustomerAddress,
                    CustomerCity = order.CustomerCity,
                    CustomerState = order.CustomerState,
                    DeliveryType = order.DeliveryTypes.Type,
                    PaymentMethodType = order.PaymentMethods.PaymentMethod,
                    Producer = order.Producer.Name,
                    CustomerEmail = order.Users.Email
                };

                return Ok(orderDto);
            }
        }

        [HttpGet]
        [ResponseType(typeof(OrderDTO))]
        [Route("api/orders/all")]
        public async Task<IHttpActionResult> GetAllOrders()
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId<0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                var orders = await entities.Orders.Where(o => o.UserId.Equals(userId)).Select(o => new OrderDTO()
                {
                    Id = o.Id,
                    Time = o.Time,
                    CustomerName = o.CustomerName,
                    CustomerAddress = o.CustomerAddress,
                    CustomerCity = o.CustomerCity,
                    CustomerState = o.CustomerState,
                    DeliveryType = o.DeliveryTypes.Type,
                    PaymentMethodType = o.PaymentMethods.PaymentMethod,
                    Producer = o.Producer.Name,
                    CustomerEmail = o.Users.Email
                }).ToListAsync();

                if (producerId > 0)
                {
                    orders = await entities.Orders.Where(o => o.ProducerId == producerId).Select(o => new OrderDTO()
                    {
                        Id = o.Id,
                        Time = o.Time,
                        CustomerName = o.CustomerName,
                        CustomerAddress = o.CustomerAddress,
                        CustomerCity = o.CustomerCity,
                        CustomerState = o.CustomerState,
                        DeliveryType = o.DeliveryTypes.Type,
                        PaymentMethodType = o.PaymentMethods.PaymentMethod,
                        Producer = o.Producer.Name,
                        CustomerEmail = o.Users.Email
                    }).ToListAsync();
                }

                return Ok(orders);
            }
        }

        [HttpPost]
        [Route("api/orders/post")]
        public async Task<IHttpActionResult> PostOrder([FromBody]Orders order)
        {

            if (order == null)
            {
                return BadRequest("Invalid JSON. Order object is null.");
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                // prevents situation where user buys from himself
                if (order.ProducerId == producerId)
                {
                    return BadRequest("Not authorized!");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid JSON.");
                }

                order.Users = entities.Users.SingleOrDefault(u => u.Id.Equals(userId));
                order.UserId = userId;
                order.Time = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                order.CustomerName = entities.Users.SingleOrDefault(u => u.Id.Equals(userId)).Name + " " +
                                     entities.Users.SingleOrDefault(u => u.Id.Equals(userId)).Surname;
                order.PaymentMethods = entities.PaymentMethods.SingleOrDefault(p => p.Id == order.PaymentMethodTypeId);
                order.DeliveryTypes = entities.DeliveryTypes.SingleOrDefault(d => d.Id == order.DeliveryTypeId);

                if (order.PaymentMethods == null || order.DeliveryTypes == null)
                {
                    return BadRequest("Invalid JSON! Payment and/or delivery incorrect!");
                }

                entities.Orders.Add(order);

                await entities.SaveChangesAsync();

                entities.Entry(order).Reference(o => o.Producer).Load();
                entities.Entry(order).Reference(o => o.Users).Load();
                entities.Entry(order).Reference(o => o.PaymentMethods).Load();
                entities.Entry(order).Reference(o => o.DeliveryTypes).Load();

                var orderDto = new OrderDTO()
                {
                    Id = order.Id,
                    Time = order.Time,
                    CustomerName = order.CustomerName,
                    CustomerAddress = order.CustomerAddress,
                    CustomerCity = order.CustomerCity,
                    CustomerState = order.CustomerState,
                    DeliveryType = order.DeliveryTypes.Type,
                    PaymentMethodType = order.PaymentMethods.PaymentMethod,
                    Producer = order.Producer.Name,
                    CustomerEmail = order.Users.Email
                };

                return Created("", orderDto);
            }
        }

        [HttpPost]
        [Route("api/orderitems/post")]
        public async Task<IHttpActionResult> PostOrderItems([FromBody]OrderItems item)
        {

            if (item == null)
            {
                return BadRequest();
            }

            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                item.Products = entities.Products.SingleOrDefault(p => p.Id == item.ProductId);
                item.Orders = entities.Orders.SingleOrDefault(o => o.Id == item.OrderId);

                // prevents situation where user buys items from different producers 
                if (item.Orders.ProducerId == producerId || item.Products.ProducerId != item.Orders.ProducerId)
                {
                    return BadRequest("Not authorized!");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid JSON!");
                }

                if (item.Quantity > entities.Products.FirstOrDefault(p => p.Id == item.ProductId).Quantity)
                {
                    return BadRequest("Products amount at stock is less then item quantity!");
                }

                entities.OrderItems.Add(item);

                await entities.SaveChangesAsync();

                entities.Entry(item).Reference(i => i.Products).Load();
                entities.Entry(item).Reference(i => i.Orders).Load();

                var itemDto = new OrderItemDTO()
                {
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductCategory = item.Products.ProductTypes.ProductCategories.Category,
                    ProductType = item.Products.ProductTypes.Name,
                    ProductQuantity = item.Quantity,
                    Price = (decimal?)item.Products.Price ?? 0,
                    Note = item.Note
                };

                return Created("", itemDto);
            }
        }

        [HttpPut]
        [Route("api/orders/update/{orderId}")]
        public async Task<IHttpActionResult> UpdateOrder(int orderId, [FromBody]Orders newOrder)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {

                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (orderId < 1)
                {
                    return NotFound();
                }

                if (newOrder == null)
                {
                    return BadRequest("Invalid JSON! Order object is null.");
                }

                var order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }

                if (!userId.Equals(order.UserId))
                {
                    return BadRequest("Not authorized!");
                }

                order.CustomerAddress = newOrder.CustomerAddress;
                order.CustomerCity = newOrder.CustomerCity;
                order.CustomerState = newOrder.CustomerState;
                order.DeliveryTypes = entities.DeliveryTypes.FirstOrDefault(d => d.Id == newOrder.DeliveryTypeId);
                order.DeliveryTypeId = newOrder.DeliveryTypeId;
                order.PaymentMethods = entities.PaymentMethods.FirstOrDefault(p => p.Id == newOrder.PaymentMethodTypeId);
                order.PaymentMethodTypeId = newOrder.PaymentMethodTypeId;

                order.CustomerName = order.Users.Name + " " + order.Users.Surname;

                await entities.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpPut]
        [Route("api/orderitems/update/{orderId}/{productId}")]
        public async Task<IHttpActionResult> UpdateOrderItem(int orderId, int productId, [FromBody]OrderItems newItem)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.Single(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.Single(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (orderId < 1 || productId < 1)
                {
                    return NotFound();
                }

                if (newItem == null)
                {
                    return BadRequest("Invalid JSON! Object is null.");
                }

                var order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId);
                var item = await entities.OrderItems.SingleOrDefaultAsync(i => i.OrderId == orderId && i.ProductId == productId);

                if (order == null || item == null)
                {
                    return NotFound();
                }

                newItem.Products = await entities.Products.FirstOrDefaultAsync(p => p.Id == productId);

                if (!userId.Equals(order.UserId) || newItem.Products.ProducerId != order.ProducerId)
                {
                    return BadRequest("Not authorized!");
                }

                if (newItem.Quantity > entities.Products.FirstOrDefault(p => p.Id == productId).Quantity)
                {
                    return BadRequest("Products amount at stock is less then item quantity!");
                }

                item.Quantity = newItem.Quantity;
                item.Note = newItem.Note;

                await entities.SaveChangesAsync();
                return Ok();
            }
        }

        [HttpGet]
        [ResponseType(typeof(OrderItemDTO))]
        [Route("api/orderitems/{orderId}/{productId}")]
        public async Task<IHttpActionResult> GetOrderItemByOrderIdAndProductId(int orderId, int productId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (orderId < 1 || productId < 1)
                {
                    return NotFound();
                }

                var order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId);
                var item = await entities.OrderItems.SingleOrDefaultAsync(i => i.OrderId == orderId && i.ProductId == productId);

                if (order == null || item == null)
                {
                    return BadRequest("Order not found!");
                }

                if (!userId.Equals(order.UserId) && !(order.ProducerId==producerId))
                {
                    return BadRequest("Not authorized!");
                }

                OrderItemDTO orderItemDto = new OrderItemDTO()
                {
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductCategory = item.Products.ProductTypes.ProductCategories.Category,
                    ProductType = item.Products.ProductTypes.Name,
                    ProductQuantity = item.Quantity,
                    Price = (decimal?)item.Products.Price ?? 0,
                    Note = item.Note
                };

                return Ok(orderItemDto);
            }
        }

        [HttpGet]
        [ResponseType(typeof(OrderItemDTO))]
        [Route("api/orderitems/{orderId}")]
        public async Task<IHttpActionResult> GetOrderItemsForOrder(int orderId)
        {
            using (eFarmDataEntities entities = new eFarmDataEntities())
            {
                int producerId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).ProducerId;
                string userId = entities.Users.SingleOrDefault(u => u.UserName.Equals(User.Identity.Name)).Id;

                if (producerId < 0 || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Authorization failed!");
                }

                if (orderId < 1)
                {
                    return NotFound();
                }

                var order = await entities.Orders.SingleOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return BadRequest("Order not found!");
                }

                if (!userId.Equals(order.UserId) && !(order.ProducerId == producerId))
                {
                    return BadRequest("Not authorized!");
                }

                var orderItems = await entities.OrderItems.Where(o => o.OrderId == orderId).Select(item => new OrderItemDTO()
                {
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductCategory = item.Products.ProductTypes.ProductCategories.Category,
                    ProductType = item.Products.ProductTypes.Name,
                    ProductQuantity = item.Quantity,
                    Price = (decimal?)item.Products.Price ?? 0,
                    Note = item.Note
                }).ToListAsync();

                return Ok(orderItems);
            }
        }
    }
}
