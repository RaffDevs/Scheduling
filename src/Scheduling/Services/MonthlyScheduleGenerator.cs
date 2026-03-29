using Scheduling.Domain.Entities;

namespace Scheduling.Scheduling.Services;

public class MonthlyScheduleGenerator
{
    public List<ScheduleEntry> Generate(
        int month,
        int year,
        List<Person> people,
        List<ScheduleEntry>? previousEntries = null,
        int scheduleId = 0)
    {
        var sundays = GetSundays(month, year);
        var activePeople = people
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToList();

        if (sundays.Count == 0 || activePeople.Count == 0)
        {
            return [];
        }

        previousEntries ??= [];

        var participationCount = activePeople.ToDictionary(
            person => person.Id,
            person => previousEntries.Count(entry => entry.PersonId == person.Id));

        var lastParticipationDate = activePeople.ToDictionary(
            person => person.Id,
            person => previousEntries
                .Where(entry => entry.PersonId == person.Id)
                .Select(entry => (DateTime?)entry.Date)
                .OrderByDescending(date => date)
                .FirstOrDefault());

        var generatedEntries = new List<ScheduleEntry>();

        foreach (var sunday in sundays)
        {
            var person = ChoosePerson(activePeople, generatedEntries, participationCount, lastParticipationDate);

            generatedEntries.Add(new ScheduleEntry
            {
                ScheduleId = scheduleId,
                Date = sunday,
                PersonId = person.Id
            });

            participationCount[person.Id]++;
            lastParticipationDate[person.Id] = sunday;
        }

        return generatedEntries;
    }

    private static Person ChoosePerson(
        List<Person> activePeople,
        List<ScheduleEntry> generatedEntries,
        Dictionary<int, int> participationCount,
        Dictionary<int, DateTime?> lastParticipationDate)
    {
        var recentWindow = GetRecentWindow(activePeople.Count);
        var recentPersonIds = generatedEntries
            .OrderByDescending(x => x.Date)
            .Take(recentWindow)
            .Select(x => x.PersonId)
            .ToHashSet();

        var availablePeople = activePeople
            .Where(person => !recentPersonIds.Contains(person.Id))
            .ToList();

        if (availablePeople.Count == 0)
        {
            availablePeople = activePeople;
        }

        return availablePeople
            .OrderBy(person => participationCount[person.Id])
            .ThenBy(person => lastParticipationDate[person.Id] ?? DateTime.MinValue)
            .ThenBy(person => person.Name)
            .First();
    }

    private static int GetRecentWindow(int peopleCount)
    {
        if (peopleCount >= 4)
        {
            return 2;
        }

        if (peopleCount >= 2)
        {
            return 1;
        }

        return 0;
    }

    private static List<DateTime> GetSundays(int month, int year)
    {
        var sundays = new List<DateTime>();
        var date = new DateTime(year, month, 1);

        while (date.Month == month)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                sundays.Add(date);
            }

            date = date.AddDays(1);
        }

        return sundays;
    }
}
