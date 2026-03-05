# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/services/ClientService/ClientService.Domain/ClientService.Domain.csproj            src/services/ClientService/ClientService.Domain/
COPY src/services/ClientService/ClientService.Application/ClientService.Application.csproj  src/services/ClientService/ClientService.Application/
COPY src/services/ClientService/ClientService.Infrastructure/ClientService.Infrastructure.csproj src/services/ClientService/ClientService.Infrastructure/
COPY src/services/ClientService/ClientService.Api/ClientService.Api.csproj                  src/services/ClientService/ClientService.Api/
RUN dotnet restore src/services/ClientService/ClientService.Api/ClientService.Api.csproj

COPY . .
RUN dotnet publish src/services/ClientService/ClientService.Api/ClientService.Api.csproj -c Release -o /app/publish

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ClientService.Api.dll"]
