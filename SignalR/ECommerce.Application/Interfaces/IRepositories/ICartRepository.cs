using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces.IRepositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByCustomerIdAsync(int customerId);

        Task<CartProduct?> GetCartProductAsync(
            int cartId,
            int productId);

        Task CreateAsync(Cart cart);

        Task AddCartProductAsync(CartProduct cartProduct);

        void RemoveCartProduct(CartProduct cartProduct);

        Task<int> CommitAsync();
    }
}
