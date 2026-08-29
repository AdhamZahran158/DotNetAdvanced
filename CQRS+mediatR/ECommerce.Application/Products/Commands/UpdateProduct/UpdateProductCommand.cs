using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.UpdateProduct
{
    public record UpdateProductCommand(
    int Id,
    string Name,
    string SKU,
    decimal Price,
    int StockQuantity
) : IRequest<bool>;
}
