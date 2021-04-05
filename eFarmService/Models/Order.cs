using eFarmDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFarmService.Models
{
    class OrderDTO
    {
        public int Id { get; set; }
        public System.DateTime Time { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string DeliveryType { get; set; }
        public string PaymentMethodType { get; set; }
        public string Producer { get; set; }
        public string CustomerEmail { get; set; }
    }

    class OrderItemDTO
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductCategory { get; set; }
        public string ProductType { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; }
    }
}
