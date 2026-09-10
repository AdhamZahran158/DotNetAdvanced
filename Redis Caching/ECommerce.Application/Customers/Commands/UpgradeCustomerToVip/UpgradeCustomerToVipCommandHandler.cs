using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Customers.Commands.UpgradeCustomerToVip
{
    public class UpgradeCustomerToVipCommandHandler
    : IRequestHandler<UpgradeCustomerToVipCommand>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpgradeCustomerToVipCommandHandler(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task Handle(
            UpgradeCustomerToVipCommand request,
            CancellationToken cancellationToken)
        {
            var customer =
                await _customerRepository
                    .GetWithOrdersAsync(request.CustomerId);

            if (customer is null)
            {
                throw new KeyNotFoundException(
                    $"Customer with ID " +
                    $"{request.CustomerId} not found.");
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
