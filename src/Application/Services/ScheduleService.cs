using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;
using Scheduling.Infrastructure;
using Scheduling.Scheduling.Services;

namespace Scheduling.Application.Services;

public class ScheduleService
{
    private readonly SchedulingDbContext _dbContext;
    private readonly MonthlyScheduleGenerator _generator;

    public ScheduleService(SchedulingDbContext dbContext, MonthlyScheduleGenerator generator)
    {
        _dbContext = dbContext;
        _generator = generator;
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        return await _dbContext.Schedules
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();
    }

    public async Task<Schedule?> GetByIdAsync(int id)
    {
        return await _dbContext.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Schedule?> GetByMonthYearAsync(int month, int year)
    {
        return await _dbContext.Schedules
            .FirstOrDefaultAsync(x => x.Month == month && x.Year == year);
    }

    public async Task<Schedule?> GetLatestAsync()
    {
        return await _dbContext.Schedules
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .FirstOrDefaultAsync();
    }

    public async Task<Schedule> CreateAsync(int month, int year)
    {
        var existingSchedule = await _dbContext.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Month == month && x.Year == year);

        if (existingSchedule is not null)
        {
            throw new InvalidOperationException("Já existe uma escala para este mês.");
        }

        var schedule = new Schedule
        {
            Month = month,
            Year = year,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Schedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        return schedule;
    }

    public async Task<ScheduleEntry> AddEntryAsync(int scheduleId, DateTime date, int personId)
    {
        var entry = new ScheduleEntry
        {
            ScheduleId = scheduleId,
            Date = date,
            PersonId = personId
        };

        _dbContext.ScheduleEntries.Add(entry);
        await _dbContext.SaveChangesAsync();

        return entry;
    }

    public async Task<List<ScheduleEntry>> GetEntriesAsync(int scheduleId)
    {
        return await _dbContext.ScheduleEntries
            .AsNoTracking()
            .Where(x => x.ScheduleId == scheduleId)
            .OrderBy(x => x.Date)
            .ToListAsync();
    }

    public async Task<Schedule> GenerateMonthlyScheduleAsync(int month, int year)
    {
        var activePeople = await _dbContext.People
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        if (activePeople.Count == 0)
        {
            throw new InvalidOperationException("Nenhuma pessoa ativa foi encontrada.");
        }

        var existingSchedule = await _dbContext.Schedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Month == month && x.Year == year);

        if (existingSchedule is not null)
        {
            throw new InvalidOperationException("Já existe uma escala para este mês.");
        }

        var previousEntries = await _dbContext.ScheduleEntries
            .AsNoTracking()
            .OrderByDescending(x => x.Date)
            .ToListAsync();

        var schedule = new Schedule
        {
            Month = month,
            Year = year,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Schedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        var generatedEntries = _generator.Generate(month, year, activePeople, previousEntries, schedule.Id);

        _dbContext.ScheduleEntries.AddRange(generatedEntries);
        await _dbContext.SaveChangesAsync();

        return schedule;
    }

    public async Task<bool> DeleteAsync(int scheduleId)
    {
        var schedule = await _dbContext.Schedules
            .FirstOrDefaultAsync(x => x.Id == scheduleId);

        if (schedule is null)
        {
            return false;
        }

        var entries = await _dbContext.ScheduleEntries
            .Where(x => x.ScheduleId == scheduleId)
            .ToListAsync();

        _dbContext.ScheduleEntries.RemoveRange(entries);
        _dbContext.Schedules.Remove(schedule);

        await _dbContext.SaveChangesAsync();

        return true;
    }
}
