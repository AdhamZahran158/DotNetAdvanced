using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler
    : IRequestHandler<GetCustomerByIdQuery, Customer?>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdQueryHandler(
            ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer?> Handle(
            GetCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _customerRepository
                .GetWithOrdersAsync(request.Id);
        }
    }
}
