using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Carts.Queries.GetCart
{
    public class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, CartDto?>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartQueryHandler(
            ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartDto?> Handle(
            GetCartQuery request,
            CancellationToken cancellationToken)
        {
            var cart = await _cartRepository
                .GetByCustomerIdAsync(request.CustomerId);

            if (cart is null)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            var validItems = cart.CartProducts
                .Where(cp =>
                    cp.AddedAt.AddDays(10) > now)
                .Select(cp => new CartItemDto
                {
                    CartProductId = cp.Id,
                    ProductId = cp.ProductId,
                    ProductName = cp.Product?.Name ?? string.Empty,
                    UnitPrice = cp.Product?.Price ?? 0m,
                    Quantity = cp.Quantity,
                    AddedAt = cp.AddedAt,
                    ExpiresAt = cp.AddedAt.AddDays(10)
                })
                .ToList();

            return new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                Items = validItems
            };
        }
    }
}
