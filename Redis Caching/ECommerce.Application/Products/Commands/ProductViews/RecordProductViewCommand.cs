using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.ProductViews
{
    public record RecordProductViewCommand(int ProductId) : IRequest;
}
