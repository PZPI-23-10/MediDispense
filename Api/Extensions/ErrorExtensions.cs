using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Extensions;

public static class ErrorExtensions
{
    public static void UseErrorHandler(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature == null)
                    return;

                context.Response.StatusCode = contextFeature.Error switch
                {
                    ValidationException => (int)HttpStatusCode.BadRequest,
                    NotFoundException => (int)HttpStatusCode.NotFound,
                    ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
                    UnauthorizedException => (int)HttpStatusCode.Unauthorized,

                    BadHttpRequestException => (int)HttpStatusCode.BadRequest,
                    OperationCanceledException => (int)HttpStatusCode.ServiceUnavailable,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                var errorResponse = new
                {
                    statusCode = context.Response.StatusCode,
                    message = contextFeature.Error.GetBaseException().Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            });
        });
    }
}