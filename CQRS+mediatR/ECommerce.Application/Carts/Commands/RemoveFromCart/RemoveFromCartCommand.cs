using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Carts.Commands.RemoveFromCart
{
    public record RemoveFromCartCommand(
    int CustomerId,
    int ProductId
) : IRequest;
}
