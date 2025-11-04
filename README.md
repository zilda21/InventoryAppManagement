# InventoryApp

Small web app for managing products in an inventory.  
Built with **ASP.NET Core Razor Pages**, **PostgreSQL**, and **ASP.NET Identity**.

🔗 **Live demo**  
https://inventoryappmanagement-ztcu.onrender.com


---

## What the app does (current scope)

**Authentication**
- Register / log in / log out (ASP.NET Identity)
- Password-based accounts stored in PostgreSQL

**Products**
- Products page only for authenticated users
- Create / edit / delete products (Name, Quantity, Price, AddedOn)
- Clean table layout without row buttons (separate inline forms for actions)

**Real-time updates**
- When one user adds/edits/deletes a product, other users on the Products page see the change automatically (SignalR hub `/hubs/products`).

**UI / UX**
- Two themes: **Light** and **Dark**  
  - Toggle in the header  
  - Preference saved in `localStorage`
- Two languages for basic UI text (e.g. login/register, some labels)  
  - Language toggle `EN / [second language]` in the header  
  - Choice saved in `localStorage`
- Simple, responsive layout with custom CSS

---

## Tech stack

**Backend**
- ASP.NET Core 9 Razor Pages
- Entity Framework Core with PostgreSQL (`Npgsql`)
- ASP.NET Core Identity
- SignalR for real-time updates

**Frontend**
- Razor views + vanilla JavaScript
- Custom CSS (no big UI framework, just light styling)
- SignalR JavaScript client via CDN

---

## How to run locally

1. **Requirements**
   - .NET 9 SDK
   - PostgreSQL running locally

2. **Clone the repo**

   ```bash
   git clone https://github.com/zilda21/InventoryAppManagement.git
   cd InventoryAppManagement/InventoryApp
