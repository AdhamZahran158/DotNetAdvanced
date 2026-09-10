using ECommerce.Application.Chat.Commands.SendChatMessage;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ECommerce.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;

        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task JoinConversation(
            int conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupName(conversationId));
        }

        public async Task LeaveConversation(
            int conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGroupName(conversationId));
        }

        public async Task SendMessage(
            int conversationId,
            string senderId,
            string senderType,
            string content)
        {
            await _mediator.Send(
                new SendChatMessageCommand(
                    conversationId,
                    senderId,
                    senderType,
                    content));
        }

        private static string GetGroupName(
            int conversationId)
        {
            return $"conversation-{conversationId}";
        }
    }
}
