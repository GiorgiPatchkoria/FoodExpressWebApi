# FoodExpress

FoodExpress is a food-ordering application built with ASP.NET Core Web API, Entity Framework Core, and SQL Server. It includes a frontend in `wwwroot`.

Features include registration and JWT login, restaurant and menu browsing, carts, orders, addresses, password recovery, and admin management with image uploads.

## Project structure

- **FoodExpress.Core** — entities, DTOs, Identity models, enums, and repository interfaces.
- **FoodExpress.Infrastructure** — database context, repositories, migrations, file storage, and UnitOfWork.
- **FoodExpress.Services** — business logic, service interfaces, and AutoMapper profiles.
- **FoodExpress** — Web API controllers, middleware, validation, and frontend.
- **FoodExpress.Tests** — service tests with Moq and repository tests with EF Core InMemory.

## Requirements

- .NET 10 SDK
- Microsoft SQL Server or SQL Server Express LocalDB
- Visual Studio or the .NET CLI

## Configuration

Keep general settings in `FoodExpress/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JwtTokenSettings": {
    "Issuer": "FoodExpress",
    "Audience": "FoodExpress.Client",
    "ExpirationInMinutes": 30
  },
  "PasswordReset": {
    "BaseUrl": "https://localhost:7004"
  }
}
```

`Issuer` identifies the API, `Audience` identifies its client, and `ExpirationInMinutes` controls JWT lifetime. `PasswordReset:BaseUrl` must match the website address used in reset-email links.

In Visual Studio, right-click **FoodExpress.Web → Manage User Secrets** and fill `secrets.json` with your own values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FoodExpress;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtTokenSettings": {
    "SecretKey": "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_BYTES"
  },
  "EmailSettings": {
    "SenderEmail": "your-address@gmail.com",
    "SenderPassword": "YOUR_GMAIL_APP_PASSWORD"
  }
}
```

- Replace the SQL Server name with your installed instance. The example uses LocalDB and Windows authentication; `TrustServerCertificate=True` is for local development.
- Use a long, random JWT secret; do not use the example placeholder.
- Email uses Gmail SMTP on port 587 with SSL enabled. Use a Gmail app password for `SenderPassword`.


## Database and running

In Visual Studio's Package Manager Console, apply the existing migrations:

```powershell
Update-Database -Project FoodExpress.Infrastructure -StartupProject FoodExpress.Web
```

Set **FoodExpress.Web** as the startup project and Run it. 

- Website: https://localhost:7004/
- Swagger: https://localhost:7004/swagger