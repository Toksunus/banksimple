# stage 1: build l'application avec le SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/BankSimple.Domain/BankSimple.Domain.csproj            src/BankSimple.Domain/
COPY src/BankSimple.Application/BankSimple.Application.csproj  src/BankSimple.Application/
COPY src/BankSimple.Infrastructure/BankSimple.Infrastructure.csproj src/BankSimple.Infrastructure/
COPY src/BankSimple.Api/BankSimple.Api.csproj                  src/BankSimple.Api/
RUN dotnet restore src/BankSimple.Api/BankSimple.Api.csproj

COPY . .
RUN dotnet publish src/BankSimple.Api/BankSimple.Api.csproj -c Release -o /app/publish

# stage 2: créer l'image finale moins lourde avec le runtime .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BankSimple.Api.dll"]
