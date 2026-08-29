using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs
{
    public class CheckoutResultDto
    {
        public int OrderId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        public decimal Shipping { get; set; }

        public decimal Total { get; set; }

        public string TransactionReference { get; set; } = string.Empty;
    }
}
