using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Application.Queries;

public sealed class GetServiceRequestQueryHandler(
    IBeneficiaryRepository beneficiaries,
    TimeProvider timeProvider,
    PriorityCalculator priorityCalculator) : IQueryHandler<GetServiceRequestQuery, ServiceRequestResponse>
{
    public async Task<ServiceRequestResponse> HandleAsync(
        GetServiceRequestQuery query,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByRequestId(query.Id, cancellationToken)
            ?? throw new ResourceNotFoundException("Service request", query.Id.ToString());
        var request = beneficiary.Requests.Single(item => item.Id == query.Id);
        var asOfDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var priority = priorityCalculator.Calculate(beneficiary, request.RegistrationDate, asOfDate);
        return ResponseMapping.ToResponse(request, beneficiary, priority);
    }
}