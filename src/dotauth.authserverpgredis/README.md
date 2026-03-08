# dotauth.authserverpgredis (Auth server with Postgres + Redis)

## Purpose
Variant of the DotAuth server configured to use PostgreSQL for primary storage and Redis for caching/session/state.

## Quickstart
- Location: `src/dotauth.authserverpgredis/dotauth.authserverpgredis.csproj`
- Build: `dotnet build ./src/dotauth.authserverpgredis/dotauth.authserverpgredis.csproj`
- Run locally: `dotnet run --project ./src/dotauth.authserverpgredis/dotauth.authserverpgredis.csproj`

## Notes
Requires PostgreSQL and Redis instances; see appsettings and Dockerfiles for sample configuration.
