using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Application.Handlers;

public sealed class CreateServiceRequestCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IEventMediator eventMediator,
    TimeProvider timeProvider,
    PriorityCalculator priorityCalculator) : ICommandHandler<CreateServiceRequestCommand, ServiceRequestResponse>
{
    public async Task<ServiceRequestResponse> HandleAsync(
        CreateServiceRequestCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiaryId = command.Input.BeneficiaryId
            ?? throw new DomainRuleException("Beneficiary is required.");
        var beneficiary = await beneficiaries.Get(beneficiaryId, cancellationToken)
            ?? throw new ResourceNotFoundException("Beneficiary", beneficiaryId.ToString());
        var registeredOn = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var initialPriority = priorityCalculator.Calculate(beneficiary, registeredOn, registeredOn);
        var request = beneficiary.AddRequest(command.Input, registeredOn, initialPriority.TotalScore);

        await eventMediator.TriggerEvents(beneficiary.DomainEvents, cancellationToken);
        await beneficiaries.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        beneficiary.ClearDomainEvents();
        return ResponseMapping.ToResponse(request, beneficiary, initialPriority);
    }
}

public sealed class ChangeServiceRequestStatusCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IEventMediator eventMediator,
    TimeProvider timeProvider) : ICommandHandler<ChangeServiceRequestStatusCommand>
{
    public async Task HandleAsync(
        ChangeServiceRequestStatusCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.GetByRequestId(command.RequestId, cancellationToken)
            ?? throw new ResourceNotFoundException("Service request", command.RequestId.ToString());
        var nextStatus = command.Input.Status
            ?? throw new DomainRuleException("Request status is required.");
        beneficiary.ChangeRequestStatus(command.RequestId, nextStatus, timeProvider.GetUtcNow());

        await eventMediator.TriggerEvents(beneficiary.DomainEvents, cancellationToken);
        await beneficiaries.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        beneficiary.ClearDomainEvents();
    }
}