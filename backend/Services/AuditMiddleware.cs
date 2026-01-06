using System.Security.Claims;
using System.Text.Json;
using MiniBank.Data;
using MiniBank.Models;

namespace MiniBank.Services;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, BankDbContext db)
    {
        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var requestBody = context.Request.Method is "POST" or "PUT"
            ? await new StreamReader(context.Request.Body).ReadToEndAsync()
            : string.Empty;

        context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await _next(context);

        newBody.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(newBody).ReadToEndAsync();
        newBody.Seek(0, SeekOrigin.Begin);
        await newBody.CopyToAsync(originalBody);

        if (context.Response.StatusCode < 400 && context.Request.Path.StartsWithSegments("/api"))
        {
            db.AuditEvents.Add(new AuditEvent
            {
                Id = Guid.NewGuid(),
                EntityName = "Http",
                Action = context.Request.Path,
                Timestamp = DateTimeOffset.UtcNow,
                UserId = userId,
                Summary = JsonSerializer.Serialize(new
                {
                    context.Request.Method,
                    Body = requestBody,
                    Response = responseBody
                })
            });
            await db.SaveChangesAsync();
        }
    }
}
