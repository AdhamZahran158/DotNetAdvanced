using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler
    : IRequestHandler<CreateCustomerCommand, Customer>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerCommandHandler(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> Handle(
            CreateCustomerCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException(
                    "Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email) ||
                !request.Email.Contains("@"))
            {
                throw new ArgumentException(
                    "A valid email address is required.");
            }

            var email =
                request.Email.Trim().ToLower();

            var emailExists =
                await _customerRepository.GetOneAsync(
                    c => c.Email.ToLower() == email);

            if (emailExists is not null)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            var customer = new Customer
            {
                FullName = request.FullName.Trim(),
                Email = email,
                IsVip = request.IsVip
            };

            await _customerRepository.CreateAsync(customer);

            await _customerRepository.CommitAsync();

            return customer;
        }
    }
}
