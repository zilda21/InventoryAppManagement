using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
namespace InventoryApp.Hubs;

   [Authorize]
public class ProductHub : Hub
{

}
