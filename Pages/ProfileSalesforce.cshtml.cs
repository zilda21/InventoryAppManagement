using System.ComponentModel.DataAnnotations;
using InventoryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryApp.Pages
{
    [Authorize]
    public class ProfileSalesforceModel : PageModel
    {
        private readonly ISalesforceService _salesforceService;

        public ProfileSalesforceModel(ISalesforceService salesforceService)
        {
            _salesforceService = salesforceService;
        }

        [BindProperty]
        [Display(Name = "Account name (Company)")]
        public string AccountName { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public void OnGet()
        {
            if (string.IsNullOrEmpty(Email) && User?.Identity?.Name is string name)
            {
                Email = name;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _salesforceService.CreateAccountAndContactAsync(
                    AccountName,
                    FirstName,
                    LastName,
                    Email);

                TempData["SalesforceMessage"] =
                    "Record successfully created in Salesforce.";
            }
            catch (Exception ex)
            {
                TempData["SalesforceMessage"] = "Salesforce error: " + ex.Message;
            }

            return Page();
        }
    }
}
