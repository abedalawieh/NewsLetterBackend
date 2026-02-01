# Newsletter Backend (Admin + API)

The backend is an ASP.NET Core app that provides:

- Admin web UI for managing subscribers, interests, newsletters, and templates
- REST APIs for the frontend
- Email sending and template rendering

## Live URLs

- Base URL: https://newsletter-server-ryog.onrender.com/
- Admin login/dashboard: https://newsletter-server-ryog.onrender.com/admin/dashbaord
- Admin newsletters: https://newsletter-server-ryog.onrender.com/Admin/Newsletters

Admin credentials:
- Username: admin
- Password: P@$$w0rd1234

## How to use (admin)

1. Open the admin login page.
2. Sign in using the credentials above.
3. Go to the Newsletters section to create a new newsletter.
4. Select the target interests and template.
5. Send the newsletter; the system emails all matching subscribers.

## Features

- Admin dashboard with authentication
- Subscriber management
- Newsletter creation and sending
- Interest-based targeting
- HTML email templates with fallback
- Database-backed storage (SQL Server or SQLite)

## Email delivery note (Render free tier)

Render’s free tier blocks outbound SMTP ports, so email sending from this backend is blocked when hosted on Render. Use an HTTP-based email provider or a host that allows SMTP for live delivery.

## Libraries used

- .NET 8 (ASP.NET Core)
- Entity Framework Core + SQL Server + SQLite providers
- ASP.NET Core Identity
- JWT libraries (Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt)
- Swashbuckle (Swagger/OpenAPI)
- WebOptimizer (bundling/minification)
- DotNetEnv (environment variables)

## Templates

Templates are HTML files stored at:

`backend/API/wwwroot/templates/email`

Available templates (file names):
- GenericNewsletter.html
- HousesNewsletter.html
- ApartmentsNewsletter.html
- RentalNewsletter.html
- SharedOwnershipNewsletter.html
- LandSourcingNewsletter.html

Template choice:
- The system uses a specific template based on the subscriber interest.
- If no specific template is available, it falls back to `GenericNewsletter`.

## Project structure

```
backend/
  API/                   ASP.NET Core host + controllers + static files
  Application/           Use cases, DTOs, services interfaces
  Domain/                Core entities and domain interfaces
  Infrastructure/        DB access, repositories, email/template services
```

## Local dev (optional)

From `backend/API`:

```bash
dotnet restore
dotnet run
```

The API will start on the configured port.
