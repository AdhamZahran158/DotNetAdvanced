using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.FlushProductViews
{
    public record FlushProductViewsCommand : IRequest;
}
