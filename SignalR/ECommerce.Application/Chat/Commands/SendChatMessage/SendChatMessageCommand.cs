using ECommerce.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Chat.Commands.SendChatMessage
{
    public record SendChatMessageCommand(
    int ConversationId,
    string SenderId,
    string SenderType,
    string Content
) : IRequest<ChatMessageDto>;
}
