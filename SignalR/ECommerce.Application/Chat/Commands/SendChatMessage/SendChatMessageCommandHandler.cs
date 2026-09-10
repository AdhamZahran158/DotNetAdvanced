using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Chat.Commands.SendChatMessage
{
    public class SendChatMessageCommandHandler
    : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
    {
        private readonly IChatMessageStore _messageStore;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public SendChatMessageCommandHandler(
            IChatMessageStore messageStore,
            IRealtimeNotifier realtimeNotifier)
        {
            _messageStore = messageStore;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<ChatMessageDto> Handle(
            SendChatMessageCommand request,
            CancellationToken cancellationToken)
        {
            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                SenderType = request.SenderType,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            await _messageStore.AddMessageAsync(
                message,
                cancellationToken);

            var dto = new ChatMessageDto(
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.SenderType,
                message.Content,
                message.SentAt);

            await _realtimeNotifier.SendToConversationAsync(
                message.ConversationId,
                "MessageReceived",
                dto,
                cancellationToken);

            return dto;
        }
    }
}
