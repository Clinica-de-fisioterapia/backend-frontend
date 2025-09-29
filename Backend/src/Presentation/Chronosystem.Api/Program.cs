// =================================================================================
// ARQUIVO: Program.cs
// OBJETIVO: Ponto de entrada da API. Configura todos os serviços (injeção de
// dependência) e o pipeline de como as requisições HTTP são tratadas.
// =================================================================================

// --- USING STATEMENTS ---
using Chronosystem.Api.Middleware;
using Chronosystem.Application.Common.Behaviors; // Para o ValidationBehavior
using Chronosystem.Application; // Para AssemblyMarker
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Chronosystem.Infrastructure.Persistence.Repositories;
using EFCore.NamingConventions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =================================================================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS (Injeção de Dependência)
// =================================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpContextAccessor();

// --- Configuração do Swagger ---
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Tenant",
        Type = SecuritySchemeType.ApiKey,
        Description = "Insira o subdomínio do Tenant (ex: empresa_teste)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TenantId"
                }
            },
            new string[] {}
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- DbContext Multi-Tenancy ---
builder.Services.AddDbContext<ApplicationDbContext>(
    (serviceProvider, options) =>
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var tenant = httpContextAccessor.HttpContext?.Request.Headers["X-Tenant"].FirstOrDefault();

        var fullConnectionString = $"{connectionString};Search Path={tenant},public";

        options.UseNpgsql(fullConnectionString)
               .UseSnakeCaseNamingConvention();
    }
);

builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

// --- MediatR ---
// Registra todos os Handlers do projeto Application
builder.Services.AddMediatR(typeof(AssemblyMarker).Assembly);
// Adiciona o ValidationBehavior no pipeline
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


var app = builder.Build();

// =================================================================================
// 2. PIPELINE HTTP
// =================================================================================

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseMiddleware<TenantResolverMiddleware>();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddress = app.Urls.FirstOrDefault();
    if (serverAddress != null)
    {
        var swaggerUrl = $"{serverAddress}/swagger";
        Console.WriteLine("===================================================");
        Console.WriteLine($"Swagger UI disponível em: {swaggerUrl}");
        Console.WriteLine("===================================================");
    }
});

app.Run();
