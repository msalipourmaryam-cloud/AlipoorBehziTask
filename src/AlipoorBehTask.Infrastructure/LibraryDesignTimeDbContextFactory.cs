using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlipoorBehTask.Infrastructure;

public sealed class LibraryDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AlipoorBehTaskDbContext>
{
    public AlipoorBehTaskDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Database=AlipoorBehTask;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        var options = new DbContextOptionsBuilder<AlipoorBehTaskDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AlipoorBehTaskDbContext).Assembly.FullName))
            .Options;
        return new AlipoorBehTaskDbContext(options);
    }
}