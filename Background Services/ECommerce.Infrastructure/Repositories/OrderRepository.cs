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
    public class OrderRepository
    : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Order?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
