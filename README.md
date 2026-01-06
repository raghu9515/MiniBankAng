# MiniBank Reference App

This repository contains a learning-focused banking/fintech reference that pairs a .NET 8 minimal API with a lightweight Angular 17 front end. It demonstrates common concerns such as JWT-based authentication, audited API calls, secure data access, and a SQL-ready Entity Framework Core data layer.

## Backend (.NET 8 minimal API)
- Location: `backend`
- Highlights:
  - JWT authentication/authorization with role-based admin policy (`RequireAdmin`).
  - Audit middleware captures API requests/responses into the `AuditEvents` table.
  - EF Core `BankDbContext` with `Account`, `Transaction`, and `AppUser` entities; defaults to in-memory storage but includes a SQL Server connection string scaffold.
  - Seeded admin user `admin@minibank.test` / `P@ssw0rd!` and a funded account for quick demos.
  - Swagger enabled for easy exploration.

### Running the API
1. Install the .NET 8 SDK.
2. From `backend`, restore/build/run:
   ```bash
   dotnet restore
   dotnet run --launch-profile http
   ```
3. Environment configuration lives in `backend/appsettings.Development.json`; change `Jwt:Key` for local secrets and point `ConnectionStrings:SqlServer` at a real database if desired.

Key endpoints:
- `POST /api/users/register` – create a user and return a JWT.
- `POST /api/users/login` – authenticate and return a JWT.
- `GET /api/accounts` – list accounts for the caller (or all when admin).
- `POST /api/accounts` – create an account for the caller.
- `POST /api/accounts/{id}/transfer` – move funds between accounts with double-entry transaction records.
- `GET /api/audit` – admin-only audit feed of recent API calls.

## Frontend (Angular 17 standalone components)
- Location: `frontend`
- Highlights:
  - Standalone Angular components with a simple dashboard and login view.
  - `AuthService` manages JWT storage and attaches tokens via an interceptor.
  - Dashboard pulls `/api/accounts` and renders transactions.

### Running the web UI
1. Install Node.js and the Angular CLI (`npm install -g @angular/cli`).
2. From `frontend`, install dependencies and start the dev server:
   ```bash
   npm install
   npm start
   ```
3. The default API base URL is `http://localhost:5000`; adjust `frontend/src/environments/environment.ts` if the API runs elsewhere.

## Learning notes
- Swap the `UseInMemoryDatabase` call in `backend/Program.cs` for `UseSqlServer(configuration.GetConnectionString("SqlServer"))` to use SQL Server.
- The audit middleware stores request/response snapshots for successful API calls; expand it to push to observability tools.
- Role-based checks protect the audit feed; extend the policy list for finer-grained access control.
- Angular components use the built-in `HttpClient` and an interceptor to keep token plumbing minimal and transparent.
