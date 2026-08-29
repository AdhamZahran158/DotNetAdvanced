using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Carts.Commands.AddToCart
{
    public class AddToCartCommandHandler
    : IRequestHandler<AddToCartCommand>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public AddToCartCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task Handle(
            AddToCartCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var product = await _productRepository.GetOneAsync(
                p => p.Id == request.Item.ProductId);

            if (product is null)
            {
                throw new KeyNotFoundException(
                    $"Product with ID {request.Item.ProductId} not found.");
            }

            var cart = await _cartRepository
                .GetByCustomerIdAsync(request.CustomerId);

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = request.CustomerId
                };

                await _cartRepository.CreateAsync(cart);

                await _cartRepository.CommitAsync();
            }

            var existingItem =
                await _cartRepository.GetCartProductAsync(
                    cart.Id,
                    product.Id);

            if (existingItem is not null)
            {
                /*
                 * If the existing item expired,
                 * remove it and create a fresh item.
                 */
                var expirationDate =
                    existingItem.AddedAt.AddDays(10);

                if (expirationDate <= DateTime.UtcNow)
                {
                    _cartRepository.RemoveCartProduct(existingItem);

                    existingItem = null;
                }
            }

            if (existingItem is not null)
            {
                existingItem.Quantity += request.Item.Quantity;
            }
            else
            {
                var cartProduct = new CartProduct
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = request.Item.Quantity,
                    AddedAt = DateTime.UtcNow
                };

                await _cartRepository
                    .AddCartProductAsync(cartProduct);
            }

            await _cartRepository.CommitAsync();
        }
    }
}
