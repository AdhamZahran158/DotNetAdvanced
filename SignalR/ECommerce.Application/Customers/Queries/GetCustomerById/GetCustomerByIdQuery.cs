using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Customers.Queries.GetCustomerById
{
    public record GetCustomerByIdQuery(int Id)
    : IRequest<Customer?>;
}
