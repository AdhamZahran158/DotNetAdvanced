using ECommerce.API.Hubs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ECommerce.API.RealTime
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IHubContext<OrderHub> _orderHub;

        public SignalRRealtimeNotifier(
            IHubContext<ChatHub> chatHub,
            IHubContext<OrderHub> orderHub)
        {
            _chatHub = chatHub;
            _orderHub = orderHub;
        }

        public async Task SendToConversationAsync(
            int conversationId,
            string eventName,
            object data,
            CancellationToken cancellationToken = default)
        {
            await _chatHub.Clients
                .Group($"conversation-{conversationId}")
                .SendAsync(
                    eventName,
                    data,
                    cancellationToken);
        }

        public async Task SendToOrderAsync(
            int orderId,
            string eventName,
            object data,
            CancellationToken cancellationToken = default)
        {
            await _orderHub.Clients
                .Group($"order-{orderId}")
                .SendAsync(
                    eventName,
                    data,
                    cancellationToken);
        }
    }
}
