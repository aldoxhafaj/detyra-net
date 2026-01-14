# eProtokoll (.NET 8)

This solution contains:

- `EProtokoll.Api` ASP.NET Core Web API with SQLite storage
- `EProtokoll.Desktop` console client
 - `EProtokoll.Desktop` WPF desktop app (Windows only)

## Quick start

1. Restore packages

```
dotnet restore
```

2. Run API

```
dotnet run --project EProtokoll.Api
```

3. Run Desktop client

```
dotnet run --project EProtokoll.Desktop
```

The API starts with Swagger enabled.

## Default admin

- User: `admin`
- Password: `Admin123!`

Change the password and JWT key in configuration before production use.

## Configuration

`EProtokoll.Api/appsettings.json`:

- `ConnectionStrings:Default` uses SQLite file `eprotokoll.db`
- `Jwt` for token generation
- `Storage` for file storage and encryption key

## API endpoints

Base prefix: `/api/v1`

- `POST /auth/login`
- `POST /auth/refresh`
- `GET /users`
- `POST /users`
- `PUT /users/{id}`
- `DELETE /users/{id}`
- `POST /users/{id}/roles`
- `GET /institutions`
- `POST /institutions`
- `PUT /institutions/{id}`
- `DELETE /institutions/{id}`
- `GET /departments`
- `POST /departments`
- `PUT /departments/{id}`
- `DELETE /departments/{id}`
- `GET /classifications`
- `GET /protocol-books`
- `POST /protocol-books/open`
- `POST /protocol-books/close`
- `GET /letters`
- `GET /letters/{id}`
- `POST /letters`
- `PUT /letters/{id}`
- `POST /letters/{id}/assign`
- `POST /letters/{id}/status`
- `GET /letters/{id}/responses`
- `POST /letters/{id}/responses`
- `GET /letters/{id}/access`
- `POST /letters/{id}/access`
- `DELETE /letters/{id}/access/{userId}`
- `GET /letters/{id}/department-access`
- `POST /letters/{id}/department-access/{departmentId}`
- `DELETE /letters/{id}/department-access/{departmentId}`
- `POST /letters/{id}/documents`
- `POST /letters/{id}/scan`
- `GET /letters/{id}/documents`
- `GET /documents/{id}/download`
- `DELETE /documents/{id}`
- `GET /reports/summary`
- `GET /reports/overdue`
- `GET /reports/by-user`
- `GET /reports/tracking`
- `GET /reports/by-priority`
- `GET /reports/by-status`
- `GET /reports/by-department`
- `GET /notifications`
- `POST /notifications/{id}/read`
- `GET /protocol-books/{year}/print`
- `GET /protocol-books/{year}/items`

## Storage and deduplication

Files are stored on disk under `Storage` using SHA-256 hash as key.
If a file with the same hash already exists, it is reused.

Secret documents are encrypted at rest using AES with the configured key.

## Notes

- Protocol numbers are generated per year with atomic increments.
- Audit logs are written for document and letter actions.
- Desktop app uses the API and covers core operations (letters, institutions, users, documents, reports, notifications, protocol book CSV).
- Database is created automatically on first run; you can add migrations with `dotnet ef migrations add Initial`.
- If the database already exists and you add new tables (Departments), delete `eprotokoll.db` and restart the API.
- Alerts are generated in the background for overdue letters and visible in notifications.
- Web UI is available at `/` from the API project.
