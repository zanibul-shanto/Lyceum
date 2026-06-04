namespace Lyceum.Models;

public class CourseProgressItem
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string NextTask { get; set; } = string.Empty;
}
