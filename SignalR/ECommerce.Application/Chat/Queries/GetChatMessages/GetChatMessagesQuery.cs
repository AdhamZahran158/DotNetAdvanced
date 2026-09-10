using ECommerce.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Chat.Queries.GetChatMessages
{
    public record GetChatMessagesQuery(
    int ConversationId
) : IRequest<IReadOnlyList<ChatMessageDto>>;
}
