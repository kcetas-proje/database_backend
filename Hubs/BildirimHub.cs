using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace KcetasAboneApi.Hubs
{
    public class BildirimHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}