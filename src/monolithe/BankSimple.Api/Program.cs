using BankSimple.Application.Services;
using BankSimple.Domain.Interfaces;
using BankSimple.Infrastructure.Persistence;
using BankSimple.Infrastructure.Repositories;
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
       .Enrich.WithProperty("service", "banksimple-api")
       .WriteTo.Console(new JsonFormatter());
});

builder.Services.AddDbContext<BankSimpleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BankSimple")));

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<InscriptionService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<ICompteRepository, CompteRepository>();
builder.Services.AddScoped<CompteService>();
builder.Services.AddScoped<IVirementRepository, VirementRepository>();
builder.Services.AddScoped<VirementService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthorization();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>{

        options.TokenValidationParameters = new TokenValidationParameters{

            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
});


builder.Services.AddCors(options =>{
    options.AddPolicy("BankSimplePolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(options =>{
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
                Reference = new OpenApiReference{ 
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

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging(options =>{
    options.EnrichDiagnosticContext = (diag, context) =>
    {
        diag.Set("RequestHost", context.Request.Host.Value);
        diag.Set("UserAgent", context.Request.Headers.UserAgent.ToString());
    };
});

app.UseHttpMetrics(options =>{
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
