using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Domain.Events;
using AlipoorBehTask.Domain.Tools;
using AlipoorBehTask.Application;
using AlipoorBehTask.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AlipoorBehTask.Infrastructure;

public sealed class AlipoorBehTaskDbContext(DbContextOptions<AlipoorBehTaskDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceRequestEvent> ServiceRequestEvents => Set<ServiceRequestEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.HasKey(beneficiary => beneficiary.Id);
            entity.HasIndex(beneficiary => beneficiary.NationalId).IsUnique();
            entity.Property(beneficiary => beneficiary.NationalId).HasMaxLength(10).IsRequired();
            entity.Property(beneficiary => beneficiary.MonthlyIncome).HasPrecision(18, 2);
            entity.Property(beneficiary => beneficiary.MaritalStatus).HasConversion<string>().HasMaxLength(24);
            entity.Property(beneficiary => beneficiary.DisabilityType).HasConversion<string>().HasMaxLength(24);
            entity.Ignore(beneficiary => beneficiary.DomainEvents);
            entity.HasMany(beneficiary => beneficiary.Requests)
                .WithOne()
                .HasForeignKey(request => request.BeneficiaryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(beneficiary => beneficiary.Requests)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(request => request.Id);
            entity.Property(request => request.ServiceType).HasConversion<string>().HasMaxLength(40);
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(request => request.Description).HasMaxLength(1000).IsRequired();
            entity.HasIndex(request => new { request.ServiceType, request.Status, request.RegistrationDate });
        });

        modelBuilder.Entity<ServiceRequestEvent>(entity =>
        {
            entity.HasKey(domainEvent => domainEvent.Id);
            entity.Property(domainEvent => domainEvent.EventType).HasMaxLength(80).IsRequired();
            entity.Property(domainEvent => domainEvent.Details).HasMaxLength(500).IsRequired();
            entity.HasIndex(domainEvent => new { domainEvent.BeneficiaryId, domainEvent.OccurredAtUtc });
        });
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var beneficiaryEntry in ChangeTracker.Entries<Beneficiary>()
                         .Where(entry => entry.State != EntityState.Detached))
            {
                var registeredRequestIds = beneficiaryEntry.Entity.DomainEvents
                    .OfType<ServiceRequestRegisteredEvent>()
                    .Select(domainEvent => domainEvent.RequestId)
                    .ToHashSet();

                foreach (var request in beneficiaryEntry.Entity.Requests
                             .Where(request => registeredRequestIds.Contains(request.Id)))
                {
                    Entry(request).State = EntityState.Added;
                }
            }

            return await SaveChangesAsync(cancellationToken) > 0;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
            sqlException.Message.Contains("NationalId", StringComparison.OrdinalIgnoreCase))
        {
            throw new DuplicateNationalIdException();
        }
    }
}