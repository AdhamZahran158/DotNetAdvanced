using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Customers.Commands.CreateCustomer
{
    public record CreateCustomerCommand(
    string FullName,
    string Email,
    bool IsVip
) : IRequest<Customer>;
}
