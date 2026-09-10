using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ECommerce.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinOrder(
            int orderId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupName(orderId));
        }

        public async Task LeaveOrder(
            int orderId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGroupName(orderId));
        }

        private static string GetGroupName(
            int orderId)
        {
            return $"order-{orderId}";
        }
    }
}
