# stage 1: build l'application avec le SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/services/PaymentService/PaymentService.Domain/PaymentService.Domain.csproj            src/services/PaymentService/PaymentService.Domain/
COPY src/services/PaymentService/PaymentService.Application/PaymentService.Application.csproj  src/services/PaymentService/PaymentService.Application/
COPY src/services/PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj src/services/PaymentService/PaymentService.Infrastructure/
COPY src/services/PaymentService/PaymentService.Api/PaymentService.Api.csproj                  src/services/PaymentService/PaymentService.Api/
RUN dotnet restore src/services/PaymentService/PaymentService.Api/PaymentService.Api.csproj

COPY . .
RUN dotnet publish src/services/PaymentService/PaymentService.Api/PaymentService.Api.csproj -c Release -o /app/publish

# stage 2: créer l'image finale moins lourde avec le runtime .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=10s --retries=5 --start-period=10s \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
