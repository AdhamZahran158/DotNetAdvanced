using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
