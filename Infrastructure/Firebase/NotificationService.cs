namespace Infrastructure.Firebase;
using FirebaseAdmin.Messaging;
using System.Threading.Tasks;

public class NotificationService
{
    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var message = new Message
        {
            Token = token,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        // Handle response if needed
    }
}
