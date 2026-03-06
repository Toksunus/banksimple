# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/services/AccountService/AccountService.Domain/AccountService.Domain.csproj            src/services/AccountService/AccountService.Domain/
COPY src/services/AccountService/AccountService.Application/AccountService.Application.csproj  src/services/AccountService/AccountService.Application/
COPY src/services/AccountService/AccountService.Infrastructure/AccountService.Infrastructure.csproj src/services/AccountService/AccountService.Infrastructure/
COPY src/services/AccountService/AccountService.Api/AccountService.Api.csproj                  src/services/AccountService/AccountService.Api/
RUN dotnet restore src/services/AccountService/AccountService.Api/AccountService.Api.csproj

COPY . .
RUN dotnet publish src/services/AccountService/AccountService.Api/AccountService.Api.csproj -c Release -o /app/publish

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AccountService.Api.dll"]
