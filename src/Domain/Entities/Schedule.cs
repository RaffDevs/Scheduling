namespace Scheduling.Domain.Entities;

public class Schedule
{
    public int Id { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
