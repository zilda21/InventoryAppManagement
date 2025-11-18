using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryApp.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // choose where root "/" should go
            return RedirectToPage("/ProfileSalesforce");
            // or: return RedirectToPage("/Products");
        }
    }
}
