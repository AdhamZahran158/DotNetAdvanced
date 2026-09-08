using ECommerce.Application.Interfaces.IRepositories;
using ECommerce.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Commands.CancelOrder
{
    public class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(
            CancelOrderCommand request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository
                .GetWithDetailsAsync(request.OrderId);

            if (order is null)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Order is already cancelled.");
            }

            if (order.Status == OrderStatus.Paid)
            {
                foreach (var item in order.Items)
                {
                    if (item.Product is not null)
                    {
                        item.Product.StockQuantity += item.Quantity;
                    }
                }
            }

            order.Status = OrderStatus.Cancelled;

            _orderRepository.Update(order);

            await _orderRepository.CommitAsync();
        }
    }
}
