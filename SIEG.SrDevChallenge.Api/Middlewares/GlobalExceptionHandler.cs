using Microsoft.AspNetCore.Mvc;
using SIEG.SrDevChallenge.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace SIEG.SrDevChallenge.Api.Middlewares;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
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

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? 500;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Title = "Erro de Validação",
                Detail = validationEx.Message,
                Status = (int)HttpStatusCode.BadRequest,
                Instance = context.Request.Path
            },

            NotFoundException notFoundEx => new ProblemDetails
            {
                Title = "Recurso Não Encontrado",
                Detail = notFoundEx.Message,
                Status = (int)HttpStatusCode.NotFound,
                Instance = context.Request.Path
            },

            ConflictException conflictEx => new ProblemDetails
            {
                Title = "Conflito de Dados",
                Detail = conflictEx.Message,
                Status = (int)HttpStatusCode.Conflict,
                Instance = context.Request.Path
            },

            UnexpectedException unexpectedEx => new ProblemDetails
            {
                Title = "Erro Inesperado",
                Detail = "Ocorreu um erro inesperado durante o processamento da solicitação.",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Title = "Erro Interno do Servidor",
                Detail = "Ocorreu um erro interno no servidor.",
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = context.Request.Path
            }
        };
    }
}
