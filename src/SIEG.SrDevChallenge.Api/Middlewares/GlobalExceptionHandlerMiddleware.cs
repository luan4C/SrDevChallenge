using Microsoft.AspNetCore.Mvc;
using SIEG.SrDevChallenge.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace SIEG.SrDevChallenge.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(exception, context);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private static ValidationProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Title = "Erro de Validação",
                Detail = validationEx.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path,
                Errors = validationEx.Errors
            },

            NotFoundException notFoundEx => new ValidationProblemDetails
            {
                Title = "Recurso Não Encontrado",
                Detail = notFoundEx.Message,
                Status = (int)HttpStatusCode.NotFound,
                Instance = context.Request.Path
            },

            ConflictException conflictEx => new ValidationProblemDetails
            {
                Title = "Conflito de Dados",
                Detail = conflictEx.Message,
                Status = (int)HttpStatusCode.Conflict,
                Instance = context.Request.Path
            },

            UnexpectedException unexpectedEx => new ValidationProblemDetails
            {
                Title = "Erro Inesperado",
                Detail = "Ocorreu um erro inesperado durante o processamento da solicitação.",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path
            },

            _ => new ValidationProblemDetails
            {
                Title = "Erro Interno do Servidor",
                Detail =  exception?.InnerException?.Message ?? exception?.Message ?? "Ocorreu um erro interno no servidor.",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path
            }
        };
    }
}
