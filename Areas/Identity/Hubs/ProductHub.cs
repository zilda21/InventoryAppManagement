using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
namespace InventoryApp.Hubs;

   [Authorize]
public class ProductHub : Hub
{
    // empty is fine – we just need a hub type
}
