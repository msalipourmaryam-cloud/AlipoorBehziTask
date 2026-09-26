using AlipoorBehTask.Application.Tools;

namespace AlipoorBehTask.Application.Commands;

public sealed record CreateBeneficiaryCommand(CreateBeneficiaryInput Input) : ICommand;
public sealed record CreateServiceRequestCommand(CreateServiceRequestInput Input) : ICommand;
public sealed record ChangeServiceRequestStatusCommand(Guid RequestId, ChangeRequestStatusInput Input) : ICommand;