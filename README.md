# StoreManager — Project Scaffold

A small shop **inventory and sales system**, built for the *Graduate to Developer*
summer camp (Module 2). This repository is the **scaffold**: **Slice 2 — Categories,
full CRUD**, working end to end. Later slices (Auth, Products, Point of Sale,
Dashboard) build on this same structure.

The frontend uses **Material Design** via [MudBlazor](https://mudblazor.com).

## Architecture — two projects, one solution

```
Browser ──▶ Blazor page ──▶ CategoryService ──HTTP──▶ Controller ──▶ EF Core ──▶ SQL Server
        ◀──   (renders)  ◀──   (List<T>)     ◀─JSON──   (200 OK)  ◀──  (rows)  ◀──
```

| Project             | What it is             | Responsibility                              |
| ------------------- | ---------------------- | ------------------------------------------- |
| `StoreManager.Api`  | ASP.NET Web API + EF Core | Owns **all** data access. Serves JSON.   |
| `StoreManager.Web`  | Blazor WebAssembly (MudBlazor) | Displays data. Never touches the DB directly. |

The Web project talks to the API over HTTP; the API talks to the database through
`AppDbContext`. That separation is the whole point.

## The database — 5 tables, 4 relationships

`users`, `categories`, `products`, `sales`, `sale_items`. See
[`StoreManager.sql`](StoreManager.sql) for the full schema and seed data, and the
class models in `StoreManager.Api/Models/`.

## Prerequisites

- **.NET 8 SDK** — verify with `dotnet --version` (should print `8.x`)
- **A database** — pick one option below.

## Running it

You need the **API** and the **Web** app running at the same time (two terminals).

### 1. Start the database

Choose whichever fits your machine:

<details open>
<summary><b>Option A — SQL Server (the curriculum path, Windows)</b></summary>

1. Install SQL Server Developer Edition + SSMS.
2. In SSMS: connect to `localhost`, create a database named **StoreManager**.
3. Open [`StoreManager.sql`](StoreManager.sql), make sure StoreManager is selected, and Execute.

The default connection string in `StoreManager.Api/appsettings.json` already points at
`localhost` / `StoreManager` with Windows Authentication.
</details>

<details>
<summary><b>Option B — SQL Server in Docker (macOS / Linux)</b></summary>

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 -d --name storemanager-sql \
  mcr.microsoft.com/mssql/server:2022-latest
# Apple Silicon: add  --platform linux/amd64
```

Then set the connection string in `StoreManager.Api/appsettings.json` to:
`Server=localhost;Database=StoreManager;User Id=sa;Password=Your_password123;TrustServerCertificate=True`
The API creates and seeds the database automatically on first run.
</details>

<details>
<summary><b>Option C — Zero setup (SQLite, works anywhere)</b></summary>

No server needed. Run the API with the `Sqlite` provider (next step). It creates a
local `StoreManager.db` file and seeds it automatically.
</details>

### 2. Start the API

```bash
cd StoreManager.Api
dotnet run                       # uses SQL Server (Options A/B)
# — or, zero-setup (Option C): —
DatabaseProvider=Sqlite dotnet run
```

API runs at **http://localhost:5136**. Swagger UI: <http://localhost:5136/swagger>.
Verify: <http://localhost:5136/api/categories> returns the four seed categories.

### 3. Start the Web app

```bash
cd StoreManager.Web
dotnet run
```

Open **http://localhost:5114/categories** — you should see the four seed categories,
and be able to add, edit and delete them.

> The API base URL the frontend calls lives in `StoreManager.Web/wwwroot/appsettings.json`.

## Solution structure

```
StoreManager.sln
├── StoreManager.Api/            ASP.NET Web API — serves data
│   ├── Controllers/
│   │   └── CategoriesController.cs   GET/POST/PUT/DELETE + correct status codes
│   ├── Models/                       Category, Product, User, Sale, SaleItem
│   ├── Data/
│   │   └── AppDbContext.cs           the only thing that talks to the database
│   ├── appsettings.json              connection strings + DatabaseProvider
│   └── Program.cs
├── StoreManager.Web/            Blazor WebAssembly — displays data (MudBlazor)
│   ├── Pages/
│   │   ├── Home.razor                dashboard
│   │   └── Categories/
│   │       ├── Index.razor           the list (MudTable)
│   │       └── Form.razor            create / edit
│   ├── Services/
│   │   └── CategoryService.cs        the frontend's bridge to the API
│   ├── Shared/                       MainLayout, NavMenu
│   ├── Models/Category.cs
│   └── wwwroot/
└── StoreManager.sql            schema + seed data (for the SQL Server path)
```

## Your first task — trace the request

Open these in order and follow one request all the way down and back:

`Pages/Categories/Index.razor` → `Services/CategoryService.cs` →
`Controllers/CategoriesController.cs` → `Data/AppDbContext.cs` → SQL → back.

If you can describe that chain, you understand the architecture.
