# InventoryApp

Small web app for managing products in an inventory.  
Built with **ASP.NET Core Razor Pages**, **PostgreSQL**, and **ASP.NET Identity**.

🔗 **Live demo**  
https://inventoryappmanagement-ztcu.onrender.com

<img width="1919" height="915" alt="image" src="https://github.com/user-attachments/assets/29920620-960d-453a-8cfa-7ae8142c49f2" />

<img width="1919" height="850" alt="image" src="https://github.com/user-attachments/assets/6a05b15d-52ca-4a38-abe5-022287656a16" />

<img width="1919" height="920" alt="image" src="https://github.com/user-attachments/assets/76c7c551-179c-4e96-9dc1-0c6dc611a1b7" />



## What the app does 

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
   
 **What could be improved / future work**

If there is more time, these are the main directions to grow the project:

Real “inventories” instead of one global product list

Users create multiple inventories, each with its own items.

Per-user permissions

Owners & admins vs normal users, write access lists, public inventories.

Custom fields and custom IDs for items (as in the full course spec).

Admin panel.

Manage users, block/unblock, add/remove admin role.

Full-text search over inventories and items.

Social login (Google, Facebook) in addition to email/password.

More translations and better coverage of the UI by the language system.

Better visual design using a full CSS framework (e.g. Bootstrap or Tailwind) and component library.
   
