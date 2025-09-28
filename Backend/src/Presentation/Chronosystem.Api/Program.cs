// =================================================================================
// ARQUIVO: Program.cs
// OBJETIVO: Ponto de entrada da API. Configura todos os serviços (injeção de
// dependência) e o pipeline de como as requisições HTTP são tratadas.
// =================================================================================

// --- USING STATEMENTS ---
using Chronosystem.Api.Middleware;
using Chronosystem.Application.Common.Behaviors; // Adicionado para o ValidationBehavior
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Chronosystem.Infrastructure.Persistence.Repositories;
using EFCore.NamingConventions;
using FluentValidation; // Adicionado para registrar os validadores
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// =================================================================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS (Injeção de Dependência)
// =================================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Adiciona o serviço que permite acessar o HttpContext atual (e os headers).
builder.Services.AddHttpContextAccessor();

// --- Bloco de configuração do Swagger atualizado ---
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

// --- Bloco de configuração do DbContext atualizado para Multi-Tenancy ---
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


// --- Configuração da Validação e MediatR ---

// Registra todos os validadores do FluentValidation que estão no projeto Application.
builder.Services.AddValidatorsFromAssembly(Assembly.Load("Chronosystem.Application"));

// Registra o MediatR e adiciona nosso ValidationBehavior ao pipeline.
// Toda requisição MediatR passará primeiro pelo ValidationBehavior antes de chegar ao Handler.
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.Load("Chronosystem.Application"));
    
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});



var app = builder.Build();

// =================================================================================
// 2. CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÕES HTTP
// =================================================================================

// Adicione o middleware de tratamento de exceções bem no início
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Comente esta linha para facilitar os testes locais.
// app.UseHttpsRedirection();

// Middleware que valida a presença do header X-Tenant.
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