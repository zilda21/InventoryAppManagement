# InventoryApp

InventoryApp is a simple inventory management web app built with ASP.NET Core, Razor Pages, Entity Framework Core, Identity, SignalR, and PostgreSQL.

It lets a logged-in user:

- Register and log in
- Create, edit, and delete products
- See the products list update in real time using SignalR
- Store all data in a PostgreSQL database

---

## Live Demo

**Deployed on Render:**  
https://inventoryappmanagement-ztcu.onrender.com

> You must register or log in first. After login you’ll be redirected to the Products page.

---

## Tech Stack

- **Frontend:** Razor Pages (ASP.NET Core)
- **Backend:** ASP.NET Core 9
- **Auth:** ASP.NET Core Identity (Register / Login)
- **Real-time:** SignalR (broadcasts product changes)
- **Database:** PostgreSQL with Entity Framework Core
- **Deployment:** Docker + Render Web Service + Render PostgreSQL

---

## Main Features

### 1. Authentication (Identity)

- Identity pages for **Register** and **Login**
- `/` (home) redirects to the **Login page**
- Products page (`/Products`) is protected with `[Authorize]`  
  → Only authenticated users can access it

### 2. Product Management (CRUD)

Products are stored in the `Products` table with:

- `Id` (int, primary key)
- `Name` (string)
- `Quantity` (int)
- `Price` (decimal)
- `AddedOn` (DateTime)

On the **Products** page you can:

- Add a new product
- Edit an existing product (values loaded into the form)
- Delete a product
- See a list of all products in a table

### 3. Real-Time Updates (SignalR)

- A SignalR hub (`ProductHub`) is mapped at `/hubs/products`
- When a product is **added, edited, or deleted**, the server calls `ProductsChanged`
- All connected clients listen for `ProductsChanged` and **reload the page**  
  → So every open browser sees the latest product list without refreshing manually

---

## Project Requirements Mapping

This is how the app matches the original project milestones:

1. **Project scaffolding**  
   - ASP.NET Core app named `InventoryApp`
   - Identity pages for Register / Login

2. **Database configuration**  
   - `ApplicationDbContext` configured with PostgreSQL via `UseNpgsql`

3. **Product model + migration**  
   - `Product` entity with `Id`, `Name`, `Quantity`, `Price`, `AddedOn`
   - EF Core migrations created and applied

4. **Database update**  
   - `dotnet ef database update --context ApplicationDbContext` used to create the Products table

5. **UI structure**  
   - Products page (`Pages/Products.cshtml` + `Products.cshtml.cs`)
   - Nav and layout set up
   - `/Products` loads successfully after login

6. **Product CRUD**  
   - Add, list, edit, and delete implemented on the Products page

7. **SignalR integration**  
   - `ProductHub` created
   - Clients connect to `/hubs/products`
   - On any change, server broadcasts `ProductsChanged` and clients reload

8. **Authentication filter**  
   - `[Authorize]` on `ProductsModel` (Products page)
   - Root `/` redirects to `/Identity/Account/Login`

9. **Deployment to Render**  
   - App containerized with `Dockerfile`
   - Deployed as a Render Web Service
   - Connected to a Render PostgreSQL instance

---

## Running the Project Locally

### Prerequisites

- .NET 9 SDK
- PostgreSQL running locally
- `dotnet-ef` tools (`dotnet tool install -g dotnet-ef` if needed)

### 1. Clone the repo

```bash
git clone https://github.com/zilda21/InventoryAppManagement.git
cd InventoryAppManagement/InventoryApp
