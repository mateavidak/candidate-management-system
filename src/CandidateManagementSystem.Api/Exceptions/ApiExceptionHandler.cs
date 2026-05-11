using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CandidateManagementSystem.Api.Exceptions;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ApiConflictException conflict)
        {
            await WriteJsonAsync(httpContext, HttpStatusCode.Conflict,
                new { code = conflict.Code, message = conflict.Message }, cancellationToken);
            return true;
        }

        if (exception is DbUpdateException dbEx && IsPostgresUniqueViolation(dbEx))
        {
            logger.LogWarning(dbEx, "Unique constraint violation");
            await WriteJsonAsync(httpContext, HttpStatusCode.Conflict,
                new { code = "UniqueConstraint", message = "A value must be unique (e.g. email or skill name)." },
                cancellationToken);
            return true;
        }

        return false;
    }

    private static bool IsPostgresUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;

    private static async Task WriteJsonAsync(
        HttpContext httpContext,
        HttpStatusCode status,
        object body,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = (int)status;
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(body), cancellationToken);
    }
}
