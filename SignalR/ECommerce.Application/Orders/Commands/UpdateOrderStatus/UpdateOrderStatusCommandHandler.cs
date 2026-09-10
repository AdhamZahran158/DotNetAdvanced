using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            IRealtimeNotifier realtimeNotifier)
        {
            _orderRepository = orderRepository;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task Handle(
            UpdateOrderStatusCommand request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOneAsync(
                x => x.Id == request.OrderId);

            if (order is null)
            {
                throw new KeyNotFoundException(
                    $"Order {request.OrderId} was not found.");
            }

            order.Status = request.Status;

            _orderRepository.Update(order);

            await _orderRepository.CommitAsync();

            await _realtimeNotifier.SendToOrderAsync(
                request.OrderId,
                "OrderStatusChanged",
                new
                {
                    OrderId = request.OrderId,
                    Status = request.Status
                },
                cancellationToken);
        }
    }
}
