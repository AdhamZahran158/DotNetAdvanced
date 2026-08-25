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
    public class CustomerRepository
     : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Customer?> GetWithOrdersAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
