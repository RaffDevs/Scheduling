namespace Scheduling.Domain.Entities;

public class SwapHistory
{
    public int Id { get; set; }

    public int ScheduleEntryId { get; set; }

    public int OldPersonId { get; set; }

    public int NewPersonId { get; set; }

    public DateTime SwappedAt { get; set; } = DateTime.UtcNow;
}
