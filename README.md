# Fullstack.IdentityAPI

Lightweight .NET 10 Identity API for authentication and authorization. This project provides JWT-based authentication, user registration, login, token refresh, and related identity features.

## Prerequisites

- .NET 10 SDK
- Optional: Docker (to run in container)
- A relational database (e.g., SQL Server, PostgreSQL) configured via connection string

## Getting started

1. Clone the repository:

   git clone <repo-url>
   cd Fullstack.IdentityAPI

2. Restore and build:

   dotnet restore
   dotnet build

3. Configure

- Update appsettings.json or supply environment variables for:
  - ConnectionStrings:DefaultConnection
  - Jwt:Key
  - Jwt:Issuer
  - Jwt:Audience
  - Any other provider-specific settings

4. Apply database migrations (if EF Core is used):

   dotnet ef database update

5. Run the API:

   dotnet run --project ./YourApiProject.csproj

The API will run on the configured URL (see appsettings or launchSettings.json).

## Docker

Build and run the container (example):

  docker build -t fullstack-identityapi .
  docker run -e ConnectionStrings__DefaultConnection="<conn>" -e Jwt__Key="<key>" -p 5000:80 fullstack-identityapi

## Common endpoints

Note: adjust paths to match the controllers in this repository. Typical endpoints include:

- POST /api/auth/register    -- register new user
- POST /api/auth/login       -- obtain JWT access (and refresh token)
- POST /api/auth/refresh     -- refresh an access token
- POST /api/auth/logout      -- revoke refresh token / sign out

Use an HTTP client (Postman, curl) to call these endpoints. Include the access token in the Authorization header as: `Authorization: Bearer <token>`.

## Configuration & Secrets

- Do not commit secrets or production connection strings to source control.
- Use environment variables, user secrets (dotnet user-secrets), or a secure secret store in production.

## Testing

Run unit/integration tests (if present):

  dotnet test

## Contributing

Contributions, bug reports, and feature requests are welcome. Please open an issue or a pull request describing the change.

## License

Specify your project license here (e.g., MIT). If none, add one before publishing.
