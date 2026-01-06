using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MiniBank.Models;

public interface IAuditable { }

public class AuditEvent
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Summary { get; set; } = string.Empty;

    public static AuditEvent? FromEntry(EntityEntry entry, IAuditable _) => entry.State switch
    {
        EntityState.Added => new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityName = entry.Entity.GetType().Name,
            Action = "Created",
            Timestamp = DateTimeOffset.UtcNow,
            Summary = string.Join(", ", entry.Properties.Where(p => p.IsModified || entry.State == EntityState.Added)
                .Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))
        },
        EntityState.Modified => new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityName = entry.Entity.GetType().Name,
            Action = "Updated",
            Timestamp = DateTimeOffset.UtcNow,
            Summary = string.Join(", ", entry.Properties.Where(p => p.IsModified)
                .Select(p => $"{p.Metadata.Name}={p.CurrentValue}"))
        },
        EntityState.Deleted => new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityName = entry.Entity.GetType().Name,
            Action = "Deleted",
            Timestamp = DateTimeOffset.UtcNow,
            Summary = "Entity removed"
        },
        _ => null
    };
}
