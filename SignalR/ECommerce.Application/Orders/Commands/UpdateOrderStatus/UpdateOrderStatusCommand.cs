using ECommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Commands.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(
    int OrderId,
    OrderStatus Status
) : IRequest;
}
