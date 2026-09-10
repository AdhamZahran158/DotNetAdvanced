using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Utilities.Redis
{
    public class ChatMessageStore : IChatMessageStore
    {
        private const string KeyPrefix = "chat:conversation:";

        private readonly IConnectionMultiplexer _redis;

        public ChatMessageStore(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private static string GetKey(int conversationId)
        {
            return $"{KeyPrefix}{conversationId}:messages";
        }

        public async Task AddMessageAsync(
            ChatMessage message,
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            var key = GetKey(message.ConversationId);

            var json = JsonSerializer.Serialize(message);

            await database.ListRightPushAsync(
                key,
                json);

            // Keep only the latest 100 messages.
            await database.ListTrimAsync(
                key,
                -100,
                -1);
        }

        public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
        {
            var database = _redis.GetDatabase();

            var key = GetKey(conversationId);

            var values = await database.ListRangeAsync(
                key,
                0,
                -1);

            var messages = new List<ChatMessage>();

            foreach (var value in values)
            {
                var message =
                    JsonSerializer.Deserialize<ChatMessage>(
                        value.ToString());

                if (message is not null)
                {
                    messages.Add(message);
                }
            }

            return messages;
        }
    }
}
