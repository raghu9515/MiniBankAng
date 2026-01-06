using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniBank.Data;
using MiniBank.Models;
using MiniBank.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseInMemoryDatabase("minibank"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));

builder.Services.AddScoped<TokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();
app.UseMiddleware<AuditMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/users/register", async (RegisterRequest request, BankDbContext db, TokenService tokens) =>
{
    if (await db.Users.AnyAsync(u => u.Email == request.Email))
    {
        return Results.BadRequest("User already exists");
    }

    var user = new AppUser
    {
        Id = Guid.NewGuid(),
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        Role = request.IsAdmin ? "Admin" : "Customer"
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = tokens.CreateToken(user);
    return Results.Ok(new { token, user = new { user.Id, user.Email, user.Role } });
});

app.MapPost("/api/users/login", async (LoginRequest request, BankDbContext db, TokenService tokens) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = tokens.CreateToken(user);
    return Results.Ok(new { token, user = new { user.Id, user.Email, user.Role } });
});

app.MapGet("/api/accounts", [Authorize] async (BankDbContext db, ClaimsPrincipal principal) =>
{
    var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var role = principal.FindFirstValue(ClaimTypes.Role);

    var accounts = role == "Admin"
        ? await db.Accounts.Include(a => a.Transactions).ToListAsync()
        : await db.Accounts.Include(a => a.Transactions).Where(a => a.OwnerId == userId).ToListAsync();

    return Results.Ok(accounts);
});

app.MapPost("/api/accounts", [Authorize] async (CreateAccountRequest request, BankDbContext db, ClaimsPrincipal principal) =>
{
    var account = new Account
    {
        Id = Guid.NewGuid(),
        OwnerId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!),
        Name = request.Name,
        Balance = 0m
    };
    db.Accounts.Add(account);
    await db.SaveChangesAsync();
    return Results.Created($"/api/accounts/{account.Id}", account);
});

app.MapPost("/api/accounts/{id:guid}/transfer", [Authorize] async (
    Guid id,
    TransferRequest request,
    BankDbContext db,
    ClaimsPrincipal principal) =>
{
    var source = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    var destination = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.DestinationAccountId);
    if (source is null || destination is null) return Results.NotFound();

    var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    if (source.OwnerId != userId)
    {
        return Results.Forbid();
    }

    if (source.Balance < request.Amount)
    {
        return Results.BadRequest("Insufficient funds");
    }

    source.Balance -= request.Amount;
    destination.Balance += request.Amount;

    db.Transactions.Add(new Transaction
    {
        Id = Guid.NewGuid(),
        AccountId = source.Id,
        Type = TransactionType.Debit,
        Amount = request.Amount,
        Description = $"Transfer to {destination.Id}",
        Timestamp = DateTimeOffset.UtcNow
    });

    db.Transactions.Add(new Transaction
    {
        Id = Guid.NewGuid(),
        AccountId = destination.Id,
        Type = TransactionType.Credit,
        Amount = request.Amount,
        Description = $"Transfer from {source.Id}",
        Timestamp = DateTimeOffset.UtcNow
    });

    await db.SaveChangesAsync();
    return Results.Ok(new { source.Balance, destination.Balance });
});

app.MapGet("/api/audit", [Authorize(Policy = "RequireAdmin")] async (BankDbContext db) =>
{
    var events = await db.AuditEvents.OrderByDescending(e => e.Timestamp).Take(100).ToListAsync();
    return Results.Ok(events);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", version = "v1" }));

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<BankDbContext>();
SeedData.Initialize(context);

app.Run();
