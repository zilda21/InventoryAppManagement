using System.ComponentModel.DataAnnotations;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Pages;

[Authorize]
public class ProfileSalesforceModel : PageModel
{
    private readonly ISalesforceService _salesforceService;
    private readonly ILogger<ProfileSalesforceModel> _logger;

    public ProfileSalesforceModel(
        ISalesforceService salesforceService,
        ILogger<ProfileSalesforceModel> logger)
    {
        _salesforceService = salesforceService;
        _logger = logger;
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
        if (string.IsNullOrEmpty(Email) && User?.Identity?.Name is string name)
            Email = name;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _salesforceService.CreateAccountAndContactAsync(
                AccountName,
                FirstName,
                LastName,
                Email);

            TempData["SalesforceMessage"] = "Salesforce Account + Contact created.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Salesforce integration failed");
            TempData["SalesforceMessage"] = "Salesforce error: " + ex.Message;
        }

        return RedirectToPage();
    }
}
