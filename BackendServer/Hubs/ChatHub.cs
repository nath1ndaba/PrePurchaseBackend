using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BackendServer.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message, DateTime time)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, time);
        }
    }
}