using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.CreateProduct
{
    public record CreateProductCommand(
    string Name,
    string SKU,
    decimal Price,
    int StockQuantity
) : IRequest<Product>;
}
