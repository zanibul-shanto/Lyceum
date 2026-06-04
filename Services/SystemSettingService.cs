using System.Text.Json;
using Lyceum.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class SystemSettingService(LyceumDbContext context)
{
    public async Task<string?> GetAsync(string key)
    {
        var setting = await context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        return setting?.SettingValue;
    }

    public async Task<List<SystemSetting>> GetAllAsync()
    {
        return await context.SystemSettings.OrderBy(s => s.SettingKey).ToListAsync();
    }

    public async Task SetAsync(string key, string? value, int? userId = null)
    {
        var existing = await context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        if (existing != null)
        {
            existing.SettingValue = value;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.SystemSettings.Add(new SystemSetting
            {
                SettingKey = key,
                SettingValue = value,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, GradeThreshold>?> GetGradingScaleAsync()
    {
        var json = await GetAsync("GradingScale");
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<Dictionary<string, GradeThreshold>>(json);
    }

    public async Task SaveGradingScaleAsync(Dictionary<string, GradeThreshold> scale, int? userId = null)
    {
        var json = JsonSerializer.Serialize(scale);
        await SetAsync("GradingScale", json, userId);
    }
}

public class GradeThreshold
{
    public int min { get; set; }
    public int max { get; set; }
    public double gpa { get; set; }
}
