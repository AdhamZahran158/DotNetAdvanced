using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductCommandHandler(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            var product =
                await _productRepository.GetOneAsync(
                    p => p.Id == request.Id);

            if (product is null)
            {
                return false;
            }

            _productRepository.Delete(product);

            await _productRepository.CommitAsync();

            return true;
        }
    }
}
