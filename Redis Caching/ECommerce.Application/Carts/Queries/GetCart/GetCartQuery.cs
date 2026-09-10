using ECommerce.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Carts.Queries.GetCart
{
    public record GetCartQuery(
    int CustomerId
) : IRequest<CartDto?>;
}
