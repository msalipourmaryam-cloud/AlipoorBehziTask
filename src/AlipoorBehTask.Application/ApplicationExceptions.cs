namespace AlipoorBehTask.Application;

public sealed class DuplicateNationalIdException
    : Exception
{
    public DuplicateNationalIdException()
        : base("A beneficiary with this national ID is already registered.")
    {
    }
}

public sealed class ResourceNotFoundException(string resource, string key)
    : Exception($"{resource} '{key}' was not found.")
{
}