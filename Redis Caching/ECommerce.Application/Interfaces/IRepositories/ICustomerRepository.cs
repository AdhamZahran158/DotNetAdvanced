using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces.IRepositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetWithOrdersAsync(int id);
    }
}
