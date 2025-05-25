using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaksIT.Core.Webapi.Middlewares;

public class TraceIdLoggingScopeMiddleware {
  private readonly RequestDelegate _next;
  private readonly ILogger<TraceIdLoggingScopeMiddleware> _logger;

  public TraceIdLoggingScopeMiddleware(RequestDelegate next, ILogger<TraceIdLoggingScopeMiddleware> logger) {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context) {
    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

    using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = traceId })) {
      await _next(context);
    }
  }
}