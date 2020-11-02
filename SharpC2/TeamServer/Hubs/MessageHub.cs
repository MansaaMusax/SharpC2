using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Shared.Models;

using System.Threading.Tasks;

namespace TeamServer.Hubs
{
    [Authorize]
    public class MessageHub : Hub
    {
        public async Task SendChatMessage(string Message)
        {
            var message = new UserMessage
            {
                Nick = Context.User.Identity.Name,
                Message = Message
            };

            await Clients.All.SendAsync("RecvMessage", message);
        }
    }
}