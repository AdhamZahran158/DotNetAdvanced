using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Queries.GetCustomerOrders
{
    public class GetCustomerOrdersQueryHandler
    : IRequestHandler<
        GetCustomerOrdersQuery,
        List<Order>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetCustomerOrdersQueryHandler(
            IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> Handle(
            GetCustomerOrdersQuery request,
            CancellationToken cancellationToken)
        {
            return await _orderRepository
                .GetByCustomerIdAsync(request.CustomerId);
        }
    }
}
