using System.ComponentModel.DataAnnotations;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryApp.Pages;

[Authorize]
public class ProfileSalesforceModel : PageModel
{
    private readonly ISalesforceService _salesforceService;

    public ProfileSalesforceModel(ISalesforceService salesforceService)
    {
        _salesforceService = salesforceService;
    }

    [BindProperty, Required]
    public string AccountName { get; set; } = "";

    [BindProperty, Required]
    public string FirstName { get; set; } = "";

    [BindProperty, Required]
    public string LastName { get; set; } = "";

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = "";

    public void OnGet()
    {
        // Pre-fill Email with current user name if appropriate
        if (string.IsNullOrEmpty(Email) && User?.Identity?.Name is string name)
            Email = name;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _salesforceService.CreateAccountAndContactAsync(
            AccountName,
            FirstName,
            LastName,
            Email);

        TempData["SalesforceMessage"] = "Salesforce Account + Contact created.";
        return RedirectToPage();
    }
}
