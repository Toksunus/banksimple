using PaymentService.Application.Services;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Formatting.Json;
using StackExchange.Redis;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, service, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithProperty("service", "payment-service")
       .WriteTo.Console(new JsonFormatter());
});

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("PaymentService.Infrastructure")));

builder.Services.AddScoped<IVirementRepository, VirementRepository>();
builder.Services.AddScoped<IVirementSagaRepository, VirementSagaRepository>();
builder.Services.AddScoped<IVirementSagaOrchestrator, VirementSagaOrchestrator>();
builder.Services.AddScoped<VirementService>();

// Register typed HttpClient for AccountService communication
var accountServiceBaseUrl = builder.Configuration["AccountService__BaseUrl"]
    ?? "http://account-service:8080";

builder.Services.AddHttpClient<IAccountServiceClient, AccountServiceHttpClient>(client =>
{
    client.BaseAddress = new Uri(accountServiceBaseUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("BankSimplePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

var kafkaBrokerUrl = builder.Configuration["Kafka:BrokerUrl"] ?? "kafka:29092";
var kafkaGroupId = builder.Configuration["Kafka:GroupId"] ?? "banksimple-payment";
var kafkaBankId = builder.Configuration["Kafka:BankId"] ?? "1";

var bbcBaseUrl = builder.Configuration["Bbc:BaseUrl"] ?? "http://host.docker.internal:5005";
var bbcBankId = int.Parse(builder.Configuration["Bbc:BankId"] ?? "1");

builder.Services.AddHttpClient<IBbcServiceClient>(client =>
{
    client.BaseAddress = new Uri(bbcBaseUrl);
}).AddTypedClient<IBbcServiceClient>((httpClient, sp) => new BbcServiceHttpClient(httpClient, bbcBankId));

builder.Services.AddHostedService(sp => new KafkaConsumer(
    kafkaBrokerUrl,
    kafkaGroupId,
    kafkaBankId,
    sp.GetRequiredService<ILogger<KafkaConsumer>>(),
    sp.GetRequiredService<IServiceScopeFactory>()));

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diag, context) =>
    {
        diag.Set("RequestHost", context.Request.Host.Value);
        diag.Set("UserAgent", context.Request.Headers.UserAgent.ToString());
    };
});

app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("endpoint", ctx => ctx.Request.Path.Value ?? "/");
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("BankSimplePolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapMetrics("/metrics");
app.MapControllers();

app.Run();
