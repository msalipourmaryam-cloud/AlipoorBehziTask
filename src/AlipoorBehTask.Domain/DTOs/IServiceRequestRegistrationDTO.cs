using AlipoorBehTask.Domain;

namespace AlipoorBehTask.Domain.DTOs;

public interface IServiceRequestRegistrationDTO
{
    ServiceType? ServiceType { get; }
    string? Description { get; }
}