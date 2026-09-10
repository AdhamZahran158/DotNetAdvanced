using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler
     : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductCommandHandler(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var existing =
                await _productRepository.GetOneAsync(
                    p => p.Id == request.Id);

            if (existing is null)
            {
                return false;
            }

            if (request.Price <= 0)
            {
                throw new ArgumentException(
                    "Price must be positive.");
            }

            if (request.StockQuantity < 0)
            {
                throw new ArgumentException(
                    "Stock quantity cannot be negative.");
            }

            existing.Name = request.Name;
            existing.SKU = request.SKU.Trim().ToUpper();
            existing.Price = request.Price;
            existing.StockQuantity = request.StockQuantity;

            _productRepository.Update(existing);

            await _productRepository.CommitAsync();

            return true;
        }
    }
}
