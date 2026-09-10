using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.DTOs
{
    public record ChatMessageDto(
    string Id,
    int ConversationId,
    string SenderId,
    string SenderType,
    string Content,
    DateTime SentAt
);
}
