using System.Text.Json;
using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class AuditLogService(LyceumDbContext context)
{
    public async Task LogAsync(int? userId, string? userRole, AuditAction action,
        string? entityName, string? entityId, object? oldValue = null, object? newValue = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserRole = userRole,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
            NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
            Timestamp = DateTime.UtcNow
        };

        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();
    }

    public async Task<(List<AuditLog> Logs, int TotalCount)> GetLogsAsync(
        int? userFilter = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        AuditAction? actionFilter = null,
        string? entityFilter = null,
        string? roleFilter = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = context.AuditLogs
            .Include(l => l.User)
            .AsQueryable();

        if (userFilter.HasValue)
            query = query.Where(l => l.UserId == userFilter.Value);
        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);
        if (actionFilter.HasValue)
            query = query.Where(l => l.Action == actionFilter.Value);
        if (!string.IsNullOrWhiteSpace(entityFilter))
            query = query.Where(l => l.EntityName == entityFilter);
        if (!string.IsNullOrWhiteSpace(roleFilter))
            query = query.Where(l => l.UserRole == roleFilter);

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalCount);
    }
}
