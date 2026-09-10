using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Queries.GetProductById
{
    public record GetProductByIdQuery(int Id)
    : IRequest<Product?>;
}
