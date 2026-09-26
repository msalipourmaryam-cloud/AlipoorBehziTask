using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;

namespace AlipoorBehTask.Infrastructure;

public sealed class DevelopmentDataSeeder(
    AlipoorBehTaskDbContext context,
    IEventMediator eventMediator,
    TimeProvider timeProvider,
    PriorityCalculator priorityCalculator)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (context.Beneficiaries.Any())
            return;

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var aggregates = new List<Beneficiary>();
        for (var index = 0; index < 50; index++)
        {
            var beneficiary = new Beneficiary(new CreateBeneficiaryInput(
                NationalId: (9000000000L + index).ToString(),
                Age: 19 + (index * 7 % 78),
                MaritalStatus: (MaritalStatus)(index % 4),
                DependentCount: index % 6,
                DisabilityType: (DisabilityType)(index % 4),
                MonthlyIncome: 500_000m + index * 375_000m));
            context.Beneficiaries.Add(beneficiary);

            var requestsForPerson = 2 + index % 5;
            for (var requestIndex = 0; requestIndex < requestsForPerson; requestIndex++)
            {
                var registrationDate = today.AddMonths(-((index + requestIndex * 3) % 19));
                var serviceType = (ServiceType)((index + requestIndex) % 3);
                var priority = priorityCalculator.Calculate(beneficiary, registrationDate, today);
                var request = beneficiary.AddRequest(
                    new CreateServiceRequestInput(
                        beneficiary.Id,
                        serviceType,
                        $"نمونه نیاز حمایتی شماره {requestIndex + 1} برای پرونده {index + 1}"),
                    registrationDate,
                    priority.TotalScore);

                switch ((index + requestIndex) % 4)
                {
                    case 1:
                        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Approved, timeProvider.GetUtcNow());
                        break;
                    case 2:
                        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Rejected, timeProvider.GetUtcNow());
                        break;
                    case 3:
                        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Approved, timeProvider.GetUtcNow());
                        beneficiary.ChangeRequestStatus(request.Id, RequestStatus.Completed, timeProvider.GetUtcNow());
                        break;
                }
            }

            aggregates.Add(beneficiary);
        }

        foreach (var aggregate in aggregates)
            await eventMediator.TriggerEvents(aggregate.DomainEvents, cancellationToken);

        await context.SaveEntitiesAsync(cancellationToken);
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}