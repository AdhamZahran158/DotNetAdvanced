using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartDto
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }

        public List<CartItemDto> Items { get; set; } = new();
    }

    public class CartItemDto
    {
        public int CartProductId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
