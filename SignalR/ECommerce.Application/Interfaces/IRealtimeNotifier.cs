using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IRealtimeNotifier
    {
        Task SendToConversationAsync(
            int conversationId,
            string eventName,
            object data,
            CancellationToken cancellationToken = default);

        Task SendToOrderAsync(
            int orderId,
            string eventName,
            object data,
            CancellationToken cancellationToken = default);
    }
}
