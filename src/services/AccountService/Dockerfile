# stage 1: build l'application avec le SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/services/AccountService/AccountService.Domain/AccountService.Domain.csproj            src/services/AccountService/AccountService.Domain/
COPY src/services/AccountService/AccountService.Application/AccountService.Application.csproj  src/services/AccountService/AccountService.Application/
COPY src/services/AccountService/AccountService.Infrastructure/AccountService.Infrastructure.csproj src/services/AccountService/AccountService.Infrastructure/
COPY src/services/AccountService/AccountService.Api/AccountService.Api.csproj                  src/services/AccountService/AccountService.Api/
RUN dotnet restore src/services/AccountService/AccountService.Api/AccountService.Api.csproj

COPY . .
RUN dotnet publish src/services/AccountService/AccountService.Api/AccountService.Api.csproj -c Release -o /app/publish

# stage 2: créer l'image finale moins lourde avec le runtime .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=10s --retries=5 --start-period=10s \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AccountService.Api.dll"]
