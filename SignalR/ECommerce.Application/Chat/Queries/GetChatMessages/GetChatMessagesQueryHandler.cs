using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Chat.Queries.GetChatMessages
{
    public class GetChatMessagesQueryHandler
    : IRequestHandler<
        GetChatMessagesQuery,
        IReadOnlyList<ChatMessageDto>>
    {
        private readonly IChatMessageStore _messageStore;

        public GetChatMessagesQueryHandler(
            IChatMessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public async Task<IReadOnlyList<ChatMessageDto>> Handle(
            GetChatMessagesQuery request,
            CancellationToken cancellationToken)
        {
            var messages =
                await _messageStore.GetMessagesAsync(
                    request.ConversationId,
                    cancellationToken);

            return messages
                .Select(x => new ChatMessageDto(
                    x.Id,
                    x.ConversationId,
                    x.SenderId,
                    x.SenderType,
                    x.Content,
                    x.SentAt))
                .ToList();
        }
    }
}
