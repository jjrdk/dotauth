dotauth.authserver (Main auth server)

Purpose
-------
A server project implementing the core DotAuth authorization server, exposing endpoints for authentication, token issuance, and configurable flows.

Quickstart
----------
- Location: `src/dotauth.authserver/dotauth.authserver.csproj`
- Build: `dotnet build ./src/dotauth.authserver/dotauth.authserver.csproj`
- Run locally: `dotnet run --project ./src/dotauth.authserver/dotauth.authserver.csproj`

Notes
-----
See repository-level Dockerfiles for production deployment. Configuration typically comes from appsettings and environment variables.

