using System.Text.Json.Serialization;
using AlipoorBehTask.Api;
using AlipoorBehTask.Api.Handlers;
using AlipoorBehTask.Application;
using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Handlers;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Events;
using AlipoorBehTask.Infrastructure;
using AlipoorBehTask.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(TimeProvider.System);

var povertyThreshold = builder.Configuration.GetValue<decimal>(
    "PriorityScoring:PovertyIncomeThreshold",
    10_000_000m);
builder.Services.AddSingleton(new PriorityCalculator(povertyThreshold));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddDbLayer(connectionString);
builder.Services.AddScoped<ICommandHandler<CreateBeneficiaryCommand, BeneficiaryResponse>, CreateBeneficiaryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateBeneficiaryCommand, BeneficiaryResponse>, UpdateBeneficiaryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteBeneficiaryCommand>, DeleteBeneficiaryCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateServiceRequestCommand, ServiceRequestResponse>, CreateServiceRequestCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangeServiceRequestStatusCommand>, ChangeServiceRequestStatusCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetBeneficiaryQuery, BeneficiaryResponse>, GetBeneficiaryQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetBeneficiariesQuery, PagedResult<BeneficiaryResponse>>, GetBeneficiariesQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetServiceRequestQuery, ServiceRequestResponse>, GetServiceRequestQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetPriorityQueueQuery, QueuePage>, GetPriorityQueueQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetRequestsSummaryQuery, RequestsSummary>, GetRequestsSummaryQueryHandler>();
builder.Services.AddScoped<IEventMediator, EventMediator>();
builder.Services.AddScoped<IEventHandler<BeneficiaryRegisteredEvent>, BeneficiaryRegisteredEventHandler>();
builder.Services.AddScoped<IEventHandler<ServiceRequestRegisteredEvent>, ServiceRequestRegisteredEventHandler>();
builder.Services.AddScoped<IEventHandler<ServiceRequestStatusChangedEvent>, ServiceRequestStatusChangedEventHandler>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AlipoorBehTaskDbContext>();
    if (database.Database.IsRelational())
        await database.Database.MigrateAsync();
    else
        await database.Database.EnsureCreatedAsync();

    if (app.Environment.IsDevelopment())
        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapControllerRoute(
    name: "dashboard",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.Run();

public partial class Program { }