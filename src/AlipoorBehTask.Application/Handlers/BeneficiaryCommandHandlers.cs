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

public sealed class UpdateBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IEventMediator eventMediator) : ICommandHandler<UpdateBeneficiaryCommand, BeneficiaryResponse>
{
    public async Task<BeneficiaryResponse> HandleAsync(
        UpdateBeneficiaryCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.Get(command.BeneficiaryId, cancellationToken)
            ?? throw new ResourceNotFoundException("Beneficiary", command.BeneficiaryId.ToString());

        var existingWithSameNationalId = await beneficiaries.GetByNationalId(
            new Beneficiary(command.Input).NationalId,
            cancellationToken);

        if (existingWithSameNationalId is not null && existingWithSameNationalId.Id != command.BeneficiaryId)
            throw new DuplicateNationalIdException();

        beneficiary.Update(command.Input);
        await eventMediator.TriggerEvents(beneficiary.DomainEvents, cancellationToken);
        await beneficiaries.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        beneficiary.ClearDomainEvents();
        return ResponseMapping.ToResponse(beneficiary);
    }
}

public sealed class DeleteBeneficiaryCommandHandler(
    IBeneficiaryRepository beneficiaries,
    IEventMediator eventMediator) : ICommandHandler<DeleteBeneficiaryCommand>
{
    public async Task HandleAsync(
        DeleteBeneficiaryCommand command,
        CancellationToken cancellationToken)
    {
        var beneficiary = await beneficiaries.Get(command.BeneficiaryId, cancellationToken)
            ?? throw new ResourceNotFoundException("Beneficiary", command.BeneficiaryId.ToString());

        beneficiaries.Remove(beneficiary);
        await eventMediator.TriggerEvents(beneficiary.DomainEvents, cancellationToken);
        await beneficiaries.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        beneficiary.ClearDomainEvents();
    }
}