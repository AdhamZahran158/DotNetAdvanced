using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByCustomerIdAsync(
            int customerId)
        {
            return await _context.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<CartProduct?> GetCartProductAsync(
            int cartId,
            int productId)
        {
            return await _context.CartProducts
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp =>
                    cp.CartId == cartId &&
                    cp.ProductId == productId);
        }

        public async Task CreateAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public async Task AddCartProductAsync(
            CartProduct cartProduct)
        {
            await _context.CartProducts.AddAsync(cartProduct);
        }

        public void RemoveCartProduct(
            CartProduct cartProduct)
        {
            _context.CartProducts.Remove(cartProduct);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
