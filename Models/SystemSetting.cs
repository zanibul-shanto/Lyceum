using System.ComponentModel.DataAnnotations;

namespace Lyceum.Models;

public class SystemSetting
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string SettingKey { get; set; } = "";

    public string? SettingValue { get; set; }

    public int? UpdatedBy { get; set; }
    public User? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
