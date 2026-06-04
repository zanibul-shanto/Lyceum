using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public enum AuditAction
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Login = 3
}

public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [StringLength(20)]
    public string? UserRole { get; set; }

    public AuditAction Action { get; set; }

    [StringLength(100)]
    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    [StringLength(50)]
    public string? IPAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
