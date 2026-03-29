using Microsoft.EntityFrameworkCore;
using Scheduling.Domain.Entities;
using Scheduling.Infrastructure;

namespace Scheduling.Application.Services;

public class PersonService
{
    private readonly SchedulingDbContext _dbContext;

    public PersonService(SchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Person>> GetAllAsync()
    {
        return await _dbContext.People
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _dbContext.People
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Person>> GetActiveAsync()
    {
        return await _dbContext.People
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Person> CreateAsync(string name, bool isActive = true)
    {
        var person = new Person
        {
            Name = name.Trim(),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.People.Add(person);
        await _dbContext.SaveChangesAsync();

        return person;
    }

    public async Task<bool> UpdateAsync(int id, string name, bool isActive)
    {
        var person = await _dbContext.People.FirstOrDefaultAsync(x => x.Id == id);

        if (person is null)
        {
            return false;
        }

        person.Name = name.Trim();
        person.IsActive = isActive;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<PersonDeleteResult> DeleteAsync(int id)
    {
        var person = await _dbContext.People.FirstOrDefaultAsync(x => x.Id == id);

        if (person is null)
        {
            return new PersonDeleteResult(false, "Pessoa não encontrada.");
        }

        var isUsedInSchedule = await _dbContext.ScheduleEntries
            .AsNoTracking()
            .AnyAsync(x => x.PersonId == id);

        var isUsedInSwapHistory = await _dbContext.SwapHistories
            .AsNoTracking()
            .AnyAsync(x => x.OldPersonId == id || x.NewPersonId == id);

        if (isUsedInSchedule || isUsedInSwapHistory)
        {
            return new PersonDeleteResult(
                false,
                "Esta pessoa já foi usada na escala. Marque como inativa em vez de excluir.");
        }

        _dbContext.People.Remove(person);
        await _dbContext.SaveChangesAsync();

        return new PersonDeleteResult(true, $"Pessoa excluída: {person.Name}");
    }
}

public record PersonDeleteResult(bool Success, string Message);
