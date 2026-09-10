using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler
        : IRequestHandler<CreateProductCommand, Product>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException(
                    "Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.SKU))
            {
                throw new ArgumentException(
                    "Product SKU is required.");
            }

            if (request.Price <= 0)
            {
                throw new ArgumentException(
                    "Product price must be greater than zero.");
            }

            if (request.StockQuantity < 0)
            {
                throw new ArgumentException(
                    "Stock quantity cannot be negative.");
            }

            var normalizedSku =
                request.SKU.Trim().ToUpper();

            var skuExists =
                await _productRepository.GetOneAsync(
                    p => p.SKU.ToUpper() == normalizedSku);

            if (skuExists is not null)
            {
                throw new InvalidOperationException(
                    $"Product with SKU '{request.SKU}' already exists.");
            }

            var product = new Product
            {
                Name = request.Name.Trim(),
                SKU = normalizedSku,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            await _productRepository.CreateAsync(product);

            await _productRepository.CommitAsync();

            return product;
        }
    }
}