using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.FlushProductViews
{
    public class FlushProductViewsCommandHandler
    : IRequestHandler<FlushProductViewsCommand>
    {
        private readonly IProductViewCounter _productViewCounter;
        private readonly IProductRepository _productRepository;

        public FlushProductViewsCommandHandler(
            IProductViewCounter productViewCounter,
            IProductRepository productRepository)
        {
            _productViewCounter = productViewCounter;
            _productRepository = productRepository;
        }

        public async Task Handle(
            FlushProductViewsCommand request,
            CancellationToken cancellationToken)
        {
            var counters =
                await _productViewCounter.GetAndResetAllAsync(
                    cancellationToken);

            foreach (var counter in counters)
            {
                var productId = counter.Key;
                var views = counter.Value;

                var product = await _productRepository.GetOneAsync(
                    p => p.Id == productId);

                if (product is null)
                    continue;

                product.TimesVisited += checked((int)views);

                _productRepository.Update(product);
            }

            await _productRepository.CommitAsync();
        }
    }
}
