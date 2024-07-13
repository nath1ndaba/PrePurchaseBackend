using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Infrastructure.Hubs
{
    public class ClockingNotificationHub : Hub
    {
        public async Task JoinCompanyGroup(string companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
        }

        public Task SendClockingNotificationToCompany(string companyId, string message)
        {
            return Clients.Group(companyId).SendAsync("ReceiveClockingNotification", message);
        }
    }


}
