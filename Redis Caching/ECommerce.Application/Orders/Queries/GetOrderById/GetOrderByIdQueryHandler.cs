using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, Order?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(
            IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Order?> Handle(
            GetOrderByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _orderRepository
                .GetWithDetailsAsync(request.Id);
        }
    }
}
