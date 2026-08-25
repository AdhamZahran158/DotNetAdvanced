using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces.IServices
{
    public interface ICustomerService
    {
        Task<Customer?> GetByIdAsync(int id);

        Task<Customer> CreateAsync(CreateCustomerDto dto);

        Task UpgradeToVipAsync(int id);
    }
}
