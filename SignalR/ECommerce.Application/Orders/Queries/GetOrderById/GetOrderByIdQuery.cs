using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(int Id)
    : IRequest<Order?>;
}
