# Build context: repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore first so the layer is cached while only source files change.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Store.Domain/Store.Domain.csproj src/Store.Domain/
COPY src/Store.Application/Store.Application.csproj src/Store.Application/
COPY src/Store.Infrastructure/Store.Infrastructure.csproj src/Store.Infrastructure/
COPY src/Store.Api/Store.Api.csproj src/Store.Api/
RUN dotnet restore src/Store.Api/Store.Api.csproj

COPY .editorconfig ./
COPY src/ src/
RUN dotnet publish src/Store.Api/Store.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# The aspnet image ships a non-root "app" user and listens on 8080.
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Store.Api.dll"]
