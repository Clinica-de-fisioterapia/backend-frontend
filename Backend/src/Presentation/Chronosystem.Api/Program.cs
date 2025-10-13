// =================================================================================
// ARQUIVO: Program.cs
// OBJETIVO: Ponto de entrada da API. Configura todos os serviços (injeção de
// dependência), multi-tenant, i18n, validação e o pipeline HTTP.
// =================================================================================

// --- USING STATEMENTS ---
using System.Globalization;
using Chronosystem.Api.Middleware;
using Chronosystem.Application.Common.Behaviors; // ValidationBehavior
using Chronosystem.Application; // AssemblyMarker
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Chronosystem.Infrastructure.Persistence.Repositories;
using EFCore.NamingConventions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Localization;
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

// --- Repositórios e Unidade de Trabalho ---
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

// --- MediatR (gratuito) ---
builder.Services.AddMediatR(typeof(AssemblyMarker).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// =================================================================================
// 2. CONFIGURAÇÃO DE LOCALIZAÇÃO (i18n)
// =================================================================================
var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// =================================================================================
// 3. CONSTRUÇÃO DA APLICAÇÃO
// =================================================================================
var app = builder.Build();

// =================================================================================
// 4. PIPELINE HTTP
// =================================================================================

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// --- Ativa localização ---
var localizationOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseMiddleware<TenantResolverMiddleware>();

app.MapControllers();

// =================================================================================
// 5. LOG DE INICIALIZAÇÃO
// =================================================================================
app.Lifetime.ApplicationStarted.Register(() =>
{
    var serverAddress = app.Urls.FirstOrDefault();
    if (serverAddress != null)
    {
        var swaggerUrl = $"{serverAddress}/swagger";
        Console.WriteLine("===================================================");
        Console.WriteLine($"🌐 Swagger UI disponível em: {swaggerUrl}");
        Console.WriteLine("🌍 Idiomas disponíveis: pt-BR, en-US");
        Console.WriteLine("===================================================");
    }
});

app.Run();
