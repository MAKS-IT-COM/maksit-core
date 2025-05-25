using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace MaksIT.Core.Webapi.Middlewares;

public class ErrorHandlingMiddleware {
  private readonly RequestDelegate _next;
  private readonly ILogger<ErrorHandlingMiddleware> _logger;

  public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger) {
    _next = next;
    _logger = logger;
  }

  public async Task Invoke(HttpContext context) {
    try {
      await _next(context); // proceed to next middleware
    }
    catch (Exception ex) {
      _logger.LogError(ex, "Unhandled exception");

      context.Response.StatusCode = 500;
      context.Response.ContentType = "application/json";

      var errorResponse = new {
        error = "An unexpected error occurred.",
        details = ex.Message // or omit in production
      };

      var json = JsonSerializer.Serialize(errorResponse);
      await context.Response.WriteAsync(json);
    }
  }
}