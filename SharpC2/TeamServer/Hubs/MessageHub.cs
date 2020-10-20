using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using System.Threading.Tasks;

namespace TeamServer.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        public async Task SendChatMessage(ChatMessage message)
        {
            await Clients.All.SendAsync("MessageIn", message);
        }
    }
}