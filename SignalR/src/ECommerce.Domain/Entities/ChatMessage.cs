using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public int ConversationId { get; set; }

        public ChatConversation Conversation { get; set; } = null!;

        public string SenderId { get; set; } = string.Empty;

        public string SenderType { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
    }
}
