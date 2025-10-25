using FluentValidation;
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

        // Verifica se a exceção é do tipo ValidationException do FluentValidation
        if (exception is ValidationException validationException)
        {
            statusCode = HttpStatusCode.BadRequest; // 400
            // Formata os erros de validação em um objeto mais amigável
            response = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1), // camelCase
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
        }
        else
        {
            // Para qualquer outra exceção não tratada, retorna um erro 500 genérico
            statusCode = HttpStatusCode.InternalServerError; // 500
            response = new { error = "An unexpected error occurred.", details = exception.Message };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}