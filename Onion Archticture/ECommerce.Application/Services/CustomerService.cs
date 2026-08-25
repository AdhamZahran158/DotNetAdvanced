using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Application.Interfaces.IServices;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _customerRepository.GetWithOrdersAsync(id);
        }

        public async Task<Customer> CreateAsync(
            CreateCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                throw new ArgumentException(
                    "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                !dto.Email.Contains("@"))
            {
                throw new ArgumentException(
                    "A valid email address is required.");
            }

            var email = dto.Email.Trim().ToLower();

            var emailExists = await _customerRepository.GetOneAsync(
                c => c.Email.ToLower() == email);

            if (emailExists is not null)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                IsVip = dto.IsVip
            };

            await _customerRepository.CreateAsync(customer);

            await _customerRepository.CommitAsync();

            return customer;
        }

        public async Task UpgradeToVipAsync(int id)
        {
            var customer =
                await _customerRepository.GetWithOrdersAsync(id);

            if (customer is null)
            {
                throw new KeyNotFoundException(
                    $"Customer with ID {id} not found.");
            }

            var totalSpent = customer.Orders
                .Where(o => o.Status == OrderStatus.Paid)
                .Sum(o => o.TotalAmount);

            const decimal vipThreshold = 500m;

            if (totalSpent < vipThreshold)
            {
                throw new InvalidOperationException(
                    $"Customer does not qualify for VIP. " +
                    $"Total spend {totalSpent:C} is less than " +
                    $"required {vipThreshold:C}.");
            }

            customer.IsVip = true;

            _customerRepository.Update(customer);

            await _customerRepository.CommitAsync();
        }
    }
}
