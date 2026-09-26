using AlipoorBehTask.Domain.IRepositories;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlipoorBehTask.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDbLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AlipoorBehTaskDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AlipoorBehTaskDbContext).Assembly.FullName)));
        services.AddScoped<IBeneficiaryRepository, EfBeneficiaryRepository>();
        services.AddScoped<IServiceRequestEventRepository, EfServiceRequestEventRepository>();
        services.AddScoped<IBeneficiaryReadStore, EfBeneficiaryReadStore>();
        services.AddScoped<IServiceRequestReadStore, EfServiceRequestReadStore>();
        services.AddScoped<DevelopmentDataSeeder>();
        return services;
    }
}