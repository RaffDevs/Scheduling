using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Services;
using Scheduling.Scheduling.Services;

namespace Scheduling.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<MonthlyScheduleGenerator>();
        services.AddScoped<PersonService>();
        services.AddScoped<ScheduleService>();
        services.AddScoped<SwapService>();

        return services;
    }
}
