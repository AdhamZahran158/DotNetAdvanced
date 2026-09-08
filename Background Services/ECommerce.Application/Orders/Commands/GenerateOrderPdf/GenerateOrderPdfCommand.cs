using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Commands.GenerateOrderPdf
{
    public record GenerateOrderPdfCommand(int OrderId, string OutputPath) : IRequest;
}
