# eProtokoll Beginner Guide (Detailed)

This guide explains the architecture, technical choices, API calls, and how to extend or integrate with other systems. It is written for someone in their first month with .NET.

## 1) What you built

You have a single solution with two projects:

- `EProtokoll.Api` is the Web API and also serves simple web pages (Razor Pages).
- `EProtokoll.Desktop` is the Windows WPF desktop app that uses the API.

The desktop app is a client. The API is the server. The API must be running before the desktop app can work.

## 2) How to run

### Run the API

```
dotnet run --project EProtokoll.Api
```

Open:
- Web pages: `http://localhost:5000/`
- Swagger: `http://localhost:5000/swagger`

### Run the desktop app (Windows only)

```
dotnet run --project EProtokoll.Desktop
```

In the desktop app set Base URL to `http://localhost:5000` and login with:
- User: `admin`
- Password: `Admin123!`

## 3) How it works (technical overview)

### API
- ASP.NET Core Web API project with controllers under `EProtokoll.Api/Controllers`.
- EF Core is used for data access with `AppDbContext`.
- SQLite is used for local data (`eprotokoll.db`).
- JWT tokens are used for authentication.
- Roles are implemented by checking `User.IsInRole`.
- Authorization rules are enforced in controllers, especially for `Secret` and `Restricted` documents.
- Background service checks overdue letters and creates notifications.

### Web pages (Razor Pages)
- Razor Pages are in `EProtokoll.Api/Pages`.
- These pages are read-only views for quick access.
- Data is loaded in page models (e.g., `Index.cshtml.cs`).

### Desktop app (WPF)
- WPF UI is defined in `MainWindow.xaml`.
- Event handlers in `MainWindow.xaml.cs` call the API with `HttpClient`.
- Responses are mapped to DTOs in `Models.cs`.
- Data is stored in `ObservableCollection` to update the UI automatically.

## 4) Architecture (simple)

This is a service-based structure inside a single API project:

- Controllers: HTTP endpoints and authorization
- Services: business logic (protocol numbers, reports, auth, notifications)
- Data: EF Core DbContext and entity models
- Storage: file system storage and encryption

This is not Clean Architecture, but it keeps responsibilities separated.

## 5) What is the hardest part

The most difficult parts are:

- Authorization rules for `Restricted` and `Secret` documents.
- Deduplication and encryption of documents on disk.
- Keeping the protocol number atomic and unique.
- Notifications and background alerts.

If you change these areas, test carefully.

## 6) How API calls are made (desktop app)

Example flow in the desktop app:

1. Login sends a POST request to `/api/v1/auth/login`.
2. The response contains `AccessToken`.
3. The token is placed in the `Authorization` header:

```
Authorization: Bearer <token>
```

4. All next API calls include that header.

You can see this in `MainWindow.xaml.cs`:
- `Login_Click` for login call
- `ApplyAuth` sets the header
- All other methods call endpoints using `_client`

## 7) How the API works internally

1. Controller receives HTTP request.
2. It checks roles and classification rules.
3. It uses EF Core to read/write data in SQLite.
4. It calls services if needed.
5. It returns JSON response.

## 8) How to integrate other systems

You already have a dedicated API endpoint for file upload and general JSON endpoints. Integration options:

- External systems can call `/api/v1/letters` to create letters.
- External systems can call `/api/v1/letters/{id}/documents` to upload files.
- Use JWT login for machine-to-machine access (create a service user).
- If you need a fixed API token, you can add a new API key authentication layer.

Typical integration steps:

1. Create a user with role `Manager` or `Employee`.
2. Call `/auth/login` to get a token.
3. Use the token in all requests.

## 9) Where to look first when you want to change something

### If you want to change API behavior

Start here:

- `EProtokoll.Api/Controllers` for endpoints
- `EProtokoll.Api/Services` for business logic
- `EProtokoll.Api/Models/Entities.cs` for database models
- `EProtokoll.Api/Data/AppDbContext.cs` for database mapping
- `EProtokoll.Api/appsettings.json` for configuration

### If you want to change the web pages

Start here:

- `EProtokoll.Api/Pages` for Razor pages
- `EProtokoll.Api/Pages/Shared/_Layout.cshtml` for layout

### If you want to change the desktop app

Start here:

- `EProtokoll.Desktop/MainWindow.xaml` for UI
- `EProtokoll.Desktop/MainWindow.xaml.cs` for API calls
- `EProtokoll.Desktop/Models.cs` for desktop DTOs

## 10) Features map (what is implemented)

### API features
- Users, roles, login, refresh
- Institutions CRUD
- Departments CRUD
- Letters (incoming, outgoing, internal)
- Protocol numbers and yearly book
- Assignments and responses
- Documents upload, scan, download, delete
- Deduplication and encryption for secret documents
- Reports (summary, overdue, by user, tracking, priority, status, department)
- Notifications and alerts

### Web pages
- Dashboard
- Letters list
- Notifications list

### Desktop app
- Letters create/update/status/assign
- Responses
- Access management (user + department)
- Institutions CRUD
- Departments CRUD
- Users create/list with department
- Documents upload/scan/list/download/delete
- Reports
- Notifications
- Protocol book open/close + CSV + print layout

## 11) Common changes (examples)

### Change API port
Edit `EProtokoll.Api/Properties/launchSettings.json`.

### Change JWT key
Edit `EProtokoll.Api/appsettings.json` under `Jwt:Key`.

### Change storage folder
Edit `EProtokoll.Api/appsettings.json` under `Storage:RootPath`.

### Add a new API endpoint
1. Add a method in a controller under `EProtokoll.Api/Controllers`.
2. Add/adjust DTOs in `EProtokoll.Api/Dtos` if needed.
3. Add logic in `EProtokoll.Api/Services` if needed.

### Add a new screen in desktop app
1. Add a new Tab in `MainWindow.xaml`.
2. Add event handlers in `MainWindow.xaml.cs`.
3. Add DTO in `Models.cs` if needed.

## 12) Database notes

- If the database already exists and you add new tables, delete `eprotokoll.db` and restart the API.
- For real use, migrate to SQL Server or PostgreSQL and add EF Core migrations.

## 13) Troubleshooting

## 14) Technical details (for learning)

### JWT authentication
- Login creates a JWT token using `JwtSecurityToken`.
- Token includes user id and role claims.
- API validates the token in `Program.cs` with `AddJwtBearer`.

### File storage
- Files are stored on disk in the `Storage` folder.
- SHA-256 hash is calculated to detect duplicates.
- Secret files are encrypted with AES before saving.
- Metadata is stored in the database.

### Protocol numbers
- Protocol book is opened per year.
- `ProtocolService` increases a counter in a transaction.
- Format is `YYYY-000001`.

### Notifications
- `AlertBackgroundService` runs every few minutes.
- It finds overdue letters and creates notifications.
- Notifications are shown in the desktop app.

### Reports
- `ReportService` queries the database and groups results.
- Reports are accessed with `/api/v1/reports/*` endpoints.

- If login fails, check API is running and Base URL is correct.
- If you get 401/403, check user roles and document classification.
- If upload fails, check Storage path and permissions.
- If HTTPS fails, use `http://localhost:5000`.
