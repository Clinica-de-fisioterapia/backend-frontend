// =================================================================================
// ARQUIVO: Program.cs
// OBJETIVO: Ponto de entrada da API. Configura todos os serviços (injeção de
// dependência), multi-tenant, i18n, validação e o pipeline HTTP.
// =================================================================================

// --- USING STATEMENTS ---
using System.Globalization;
using System.Linq;
using Chronosystem.Api.Middleware;
using Chronosystem.Application.Common.Behaviors; // ValidationBehavior
using Chronosystem.Application; // AssemblyMarker
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Chronosystem.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using System.Text.Json.Serialization;
using EmptyStringToNullableGuidConverter = Chronosystem.Infrastructure.Serialization.EmptyStringToNullableGuidConverter;

// >>> ADIÇÕES MÍNIMAS PARA JWT/REFRESH E VALIDAÇÃO DE TENANT <<<
using Chronosystem.Application.Common.Interfaces.Authentication;
using Chronosystem.Infrastructure.Authentication;
using Chronosystem.Infrastructure.Configuration;           // AuthenticationSetup / MiddlewareSetup
using Chronosystem.Infrastructure.Middleware;             // TenantValidationMiddleware
using Microsoft.Extensions.Configuration;                 // IConfiguration
using Microsoft.AspNetCore.Http;                          // IHttpContextAccessor
using Chronosystem.Infrastructure.Tenancy;                // TenantProvisioningService
using Microsoft.AspNetCore.Authorization;                 // Authorization options
using Chronosystem.Infrastructure.Security.Permissions;   // Permission policies

var builder = WebApplication.CreateBuilder(args);

// =================================================================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS (Injeção de Dependência)
// =================================================================================

// --- Controllers com suporte aos conversores ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Aceita enums por string ("Admin") ou número (0)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: true));
        // ✅ Converte "" -> null para Guid?
        options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullableGuidConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// --- Configuração do Swagger ---
builder.Services.AddSwaggerGen(options =>
{
    // Header do Tenant (mantido)
    options.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Tenant",
        Type = SecuritySchemeType.ApiKey,
        Description = "Insira o subdomínio do Tenant (ex: empresa_teste)"
    });

    // >>> Esquema Bearer para JWT <<<
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorização via JWT. Exemplo: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
            Array.Empty<string>()
        },
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

static string QuoteIdentifier(string identifier) =>
    string.Concat("\"", (identifier ?? string.Empty).Replace("\"", "\"\""), "\"");

// --- DbContext Multi-Tenancy + Enum Mapping (Npgsql DataSource) ---
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var configuration       = serviceProvider.GetRequiredService<IConfiguration>();

    var baseCnn = connectionString
        ?? configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=chronosystem;Username=postgres;Password=1234";

    // Resolve tenant do header (fallback para public)
    var tenant = httpContextAccessor.HttpContext?.Request?.Headers["X-Tenant"].FirstOrDefault();
    tenant = string.IsNullOrWhiteSpace(tenant)
        ? string.Empty
        : tenant.Trim().ToLowerInvariant();

    var searchPath = string.IsNullOrWhiteSpace(tenant)
        ? "public"
        : $"{QuoteIdentifier(tenant)},public";

    var fullConnectionString = $"{baseCnn};Search Path={searchPath}";

    options
        .UseNpgsql(fullConnectionString)
        .UseSnakeCaseNamingConvention();       // Mantém snake_case compatível com scripts
});

// --- Repositórios e Unidade de Trabalho ---
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
builder.Services.AddScoped<ITenantCatalogReader, TenantCatalogReader>();
builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

// --- MediatR ---
builder.Services.AddMediatR(typeof(AssemblyMarker).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// >>> REGISTRO JWT + REFRESH <<<
builder.Services.AddJwtAuthentication(builder.Configuration);            // Issuer/Audience/Key/Validate
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>(); // Serviço de refresh tokens

// >>> AUTORIZAÇÃO COM SUPORTE A POLÍTICAS PERSONALIZADAS <<<
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    // Role-based simples (string), mantendo extensibilidade (sem enum fixo)
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));
    options.AddPermissionPolicy("Permission:ManageUsers", "manage:users");
});

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

// Resolve tenant do header (seu middleware atual)
app.UseMiddleware<TenantResolverMiddleware>();

// >>> AUTENTICAÇÃO / VALIDAÇÃO DE TENANT / AUTORIZAÇÃO (ordem importante) <<<
app.UseAuthentication();
app.UseMiddleware<TenantValidationMiddleware>(); // compara X-Tenant com claim 'tenant' do JWT
app.UseAuthorization();

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
