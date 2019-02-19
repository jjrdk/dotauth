FROM microsoft/dotnet:2.2-sdk-alpine AS build-env
WORKDIR /app

COPY src/ ./src/
COPY tests/ ./tests/
COPY simpleauth.sln ./
COPY shared.proj ./
RUN dotnet restore ./simpleauth.sln
RUN dotnet publish -c Release -o out src/simpleauth.authserver/simpleauth.authserver.csproj

# Build runtime image
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=build-env /app/src/simpleauth.authserver/out/ .
ENTRYPOINT ["dotnet", "simpleauth.authserver.dll"]
