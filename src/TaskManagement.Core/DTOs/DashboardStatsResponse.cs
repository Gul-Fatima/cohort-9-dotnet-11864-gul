namespace TaskManagement.Core.DTOs;

/// <summary>
/// The dashboard counters. Scoped like the task list: admins get team-wide
/// numbers, regular users get their own.
/// </summary>
public class DashboardStatsResponse
{
    public int Completed { get; set; }

    public int InProgress { get; set; }

    public int Pending { get; set; }

    public int Total { get; set; }
}
