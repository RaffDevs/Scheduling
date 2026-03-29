using Scheduling.Application;
using Scheduling.Application.Services;
using Scheduling.UI.Components;
using MudBlazor.Services;
using Scheduling.Infrastructure;
using Scheduling.Print.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ScheduleSharePngService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapGet("/escala/{scheduleId:int}/imagem/preview", async (
    int scheduleId,
    bool? download,
    ScheduleService scheduleService,
    PersonService personService,
    ScheduleSharePngService scheduleSharePngService) =>
{
    var schedule = await scheduleService.GetByIdAsync(scheduleId);

    if (schedule is null)
    {
        return Results.NotFound();
    }

    var entries = await scheduleService.GetEntriesAsync(schedule.Id);
    var people = await personService.GetAllAsync();
    var fileBytes = await scheduleSharePngService.GenerateAsync(schedule.Month, schedule.Year, entries, people);
    var fileName = $"escala-{schedule.Year}-{schedule.Month:D2}.png";

    return Results.File(fileBytes, "image/png", download == true ? fileName : null);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
