using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Domain.IRepositories;

namespace AlipoorBehTask.Application.Handlers;

public sealed class CreateBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IEventMediator eventMediator) : ICommandHandler<CreateBeneficiaryCommand, BeneficiaryResponse>
{
    public async Task<BeneficiaryResponse> HandleAsync(
        CreateBeneficiaryCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiary = new Beneficiary(command.Input);
        if (await beneficiaries.GetByNationalId(beneficiary.NationalId, cancellationToken) is not null)
            throw new DuplicateNationalIdException();

        beneficiaries.Add(beneficiary);
        await eventMediator.TriggerEvents(beneficiary.DomainEvents, cancellationToken);
        await beneficiaries.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        beneficiary.ClearDomainEvents();
        return ResponseMapping.ToResponse(beneficiary);
    }
}