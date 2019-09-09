# Build runtime image
FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY artifacts/publish/ .
#COPY --from=build-env /app/src/simpleauth.authserver/out/ .
ENTRYPOINT ["dotnet", "simpleauth.authserver.dll"]
