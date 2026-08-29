using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, Product?>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product?> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _productRepository.GetOneAsync(
                p => p.Id == request.Id);
        }
    }
}
