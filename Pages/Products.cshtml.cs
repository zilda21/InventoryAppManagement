using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryApp.Data;
using InventoryApp.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Pages;

[Authorize] // Require login for Products page
public class ProductsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<ProductHub> _hub;

    public ProductsModel(ApplicationDbContext db, IHubContext<ProductHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public IList<Product> Products { get; set; } = new List<Product>();

    // Top form fields
    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public int Qty { get; set; }

    [BindProperty]
    public decimal Price { get; set; }

    // When not null → we are editing that product
    [BindProperty]
    public int? EditId { get; set; }

    public async Task OnGetAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        Products = await _db.Products
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }

    // Add OR Update (depending on EditId)
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ModelState.AddModelError(nameof(Name), "Name is required.");
        }

        if (!ModelState.IsValid)
        {
            await LoadProductsAsync();
            return Page();
        }

        if (EditId.HasValue && EditId.Value > 0)
        {
            // UPDATE
            var existing = await _db.Products.FindAsync(EditId.Value);
            if (existing != null)
            {
                existing.Name     = Name!.Trim();
                existing.Quantity = Qty;
                existing.Price    = Price;
            }
        }
        else
        {
            // INSERT
            var product = new Product
            {
                Name     = Name!.Trim(),
                Quantity = Qty,
                Price    = Price,

                // IMPORTANT FIX: save as UTC so PostgreSQL is happy
                // (your column is DateTimeOffset → Npgsql only accepts offset 0)
                AddedOn  = DateTimeOffset.UtcNow
            };

            _db.Products.Add(product);
        }

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ProductsChanged");

        // PRG pattern: avoid reposts
        return RedirectToPage();
    }

    // Load a product into the top form for editing
    public async Task<IActionResult> OnPostEditAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            EditId = product.Id;
            Name   = product.Name;
            Qty    = product.Quantity;
            Price  = product.Price;
        }

        await LoadProductsAsync();
        return Page();
    }

    // Cancel editing, clear the form (we just redirect)
    public IActionResult OnPostCancelEdit()
    {
        return RedirectToPage();
    }

    // Delete
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("ProductsChanged");
        }

        return RedirectToPage();
    }
}
