using ECommerce.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.ProductViews
{
    public class RecordProductViewCommandHandler
    : IRequestHandler<RecordProductViewCommand>
    {
        private readonly IProductViewCounter _productViewCounter;

        public RecordProductViewCommandHandler(
            IProductViewCounter productViewCounter)
        {
            _productViewCounter = productViewCounter;
        }

        public async Task Handle(
            RecordProductViewCommand request,
            CancellationToken cancellationToken)
        {
            await _productViewCounter.IncrementAsync(
                request.ProductId,
                cancellationToken);
        }
    }
}
