using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using AlipoorBehTask.Domain.IRepositories;
using Moq;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class GetRequestsSummaryHandlerTests
{
    [Fact]
    public async Task HandleAsync_IncludesZeroCountsForEveryServiceAndStatus()
    {
        var repository = new Mock<IBeneficiaryRepository>();
        repository.Setup(item => item.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IBeneficiary>());
        var handler = new GetRequestsSummaryQueryHandler(repository.Object);

        var summary = await handler.HandleAsync(new GetRequestsSummaryQuery(), CancellationToken.None);

        Assert.Equal(
            Enum.GetValues<ServiceType>().Length * Enum.GetValues<RequestStatus>().Length,
            summary.Items.Count);
        Assert.All(summary.Items, item => Assert.Equal(0, item.Count));
    }
}