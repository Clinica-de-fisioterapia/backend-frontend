using Chronosystem.Application.Common.Exceptions;
using Chronosystem.Application.Resources;
using Chronosystem.Infrastructure.Tenancy.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace Chronosystem.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Tenta executar o resto do pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Se uma exceção ocorrer, chama nosso método para tratá-la
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        object? response;

        switch (exception)
        {
            case TenantHeaderMissingException:
                statusCode = HttpStatusCode.BadRequest;
                response = new { error = Messages.Tenant_Header_Required };
                break;
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1),
                        g => g.Select(e => e.ErrorMessage).ToArray());
                break;
            case BusinessRuleException businessRuleException:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = new { error = businessRuleException.Message };
                break;
            default:
            {
                var env = context.RequestServices.GetService<IHostEnvironment>();
                var isDev = env?.IsDevelopment() ?? false;

                statusCode = HttpStatusCode.InternalServerError;
                response = isDev
                    ? new { error = Messages.Validation_Request_Invalid, details = exception.Message }
                    : new { error = Messages.Validation_Request_Invalid };

                break;
            }
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}