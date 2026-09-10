using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IChatMessageStore
    {
        Task AddMessageAsync(
            ChatMessage message,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
            int conversationId,
            CancellationToken cancellationToken = default);
    }
}
