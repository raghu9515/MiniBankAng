using Microsoft.EntityFrameworkCore;
using MiniBank.Models;

namespace MiniBank.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<AppUser> Users => Set<AppUser>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable auditable && e.State != EntityState.Unchanged)
            .Select(e => AuditEvent.FromEntry(e, auditable))
            .Where(e => e is not null)
            .Select(e => e!)
            .ToList();

        var result = base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            AuditEvents.AddRange(auditEntries);
        }

        return result;
    }
}

public static class SeedData
{
    public static void Initialize(BankDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@minibank.test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
            Role = "Admin"
        };

        db.Users.Add(admin);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = admin.Id,
            Name = "Seed Admin Checking",
            Balance = 5000m
        };

        db.Accounts.Add(account);
        db.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = 5000m,
            Type = TransactionType.Credit,
            Description = "Initial funding",
            Timestamp = DateTimeOffset.UtcNow
        });

        db.SaveChanges();
    }
}
