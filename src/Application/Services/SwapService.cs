using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;
using Scheduling.Infrastructure;

namespace Scheduling.Application.Services;

public class SwapService
{
    private readonly SchedulingDbContext _dbContext;

    public SwapService(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SwapPersonAsync(int scheduleEntryId, int newPersonId)
    {
        var entry = await _dbContext.ScheduleEntries.FirstOrDefaultAsync(x => x.Id == scheduleEntryId);

        if (entry is null)
        {
            return false;
        }

        var newPerson = await _dbContext.People.FirstOrDefaultAsync(x => x.Id == newPersonId && x.IsActive);

        if (newPerson is null)
        {
            return false;
        }

        var oldPersonId = entry.PersonId;

        entry.PersonId = newPersonId;

        var history = new SwapHistory
        {
            ScheduleEntryId = entry.Id,
            OldPersonId = oldPersonId,
            NewPersonId = newPersonId,
            SwappedAt = DateTime.UtcNow
        };

        _dbContext.SwapHistories.Add(history);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<SwapHistory>> GetHistoryAsync()
    {
        return await _dbContext.SwapHistories
            .AsNoTracking()
            .OrderByDescending(x => x.SwappedAt)
            .ToListAsync();
    }
}
