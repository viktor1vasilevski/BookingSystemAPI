using Main.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Main.Hubs
{
    public class BookingHub : Hub
    {
        public async Task SendMessage(BookingStatusEnum status, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", status, message);
        }
    }
}
