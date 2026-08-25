using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces.IServices
{
    public interface IOrderService
    {
        Task<Order?> GetByIdAsync(int id);

        Task<List<Order>> GetCustomerOrdersAsync(int customerId);

        Task CancelAsync(int id);

        Task<CheckoutResultDto> CheckoutAsync(
            CreateOrderDto request);
    }
}
