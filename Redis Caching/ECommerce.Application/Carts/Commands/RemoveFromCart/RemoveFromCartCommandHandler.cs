using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Carts.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler
    : IRequestHandler<RemoveFromCartCommand>
    {
        private readonly ICartRepository _cartRepository;

        public RemoveFromCartCommandHandler(
            ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task Handle(
            RemoveFromCartCommand request,
            CancellationToken cancellationToken)
        {
            var cart = await _cartRepository
                .GetByCustomerIdAsync(request.CustomerId);

            if (cart is null)
            {
                throw new KeyNotFoundException(
                    "Cart not found.");
            }

            var item = cart.CartProducts
                .FirstOrDefault(cp =>
                    cp.ProductId == request.ProductId);

            if (item is null)
            {
                throw new KeyNotFoundException(
                    "Product is not in the cart.");
            }

            _cartRepository.RemoveCartProduct(item);

            await _cartRepository.CommitAsync();
        }
    }
}
