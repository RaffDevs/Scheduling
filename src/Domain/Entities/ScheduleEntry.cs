namespace Scheduling.Domain.Entities;

public class ScheduleEntry
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public DateTime Date { get; set; }

    public int PersonId { get; set; }
}
