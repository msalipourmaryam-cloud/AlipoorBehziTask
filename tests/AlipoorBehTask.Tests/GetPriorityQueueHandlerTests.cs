using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Domain.Aggregates;
using Moq;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class GetPriorityQueueHandlerTests
{
    [Fact]
    public async Task HandleAsync_PreservesServerPriorityOrderAcrossPages()
    {
        var olderBeneficiary = CreateBeneficiary("0000000001", 70, DisabilityType.None);
        var highBeneficiary = CreateBeneficiary("0000000002", 71, DisabilityType.Severe);
        var tieBeneficiaryA = CreateBeneficiary("0000000003", 70, DisabilityType.None);
        var tieBeneficiaryB = CreateBeneficiary("0000000004", 70, DisabilityType.None);
        var olderRequest = CreateRequest(olderBeneficiary, ServiceType.Pension, new DateOnly(2025, 1, 1), "Older request");
        var highPriorityRequest = CreateRequest(highBeneficiary, ServiceType.Wheelchair, new DateOnly(2025, 2, 15), "High priority request");
        var tieRequestA = CreateRequest(tieBeneficiaryA, ServiceType.HousingDepositLoan, new DateOnly(2025, 1, 15), "Tie A");
        var tieRequestB = CreateRequest(tieBeneficiaryB, ServiceType.Pension, new DateOnly(2025, 1, 15), "Tie B");

        var firstPage = new PagedResult<ServiceRequestQueueRecord>(1, 2, 4,
        [
            new ServiceRequestQueueRecord(highPriorityRequest, highBeneficiary),
            new ServiceRequestQueueRecord(olderRequest, olderBeneficiary)
        ]);
        var tiedRows = new[]
        {
            new ServiceRequestQueueRecord(tieRequestA, tieBeneficiaryA),
            new ServiceRequestQueueRecord(tieRequestB, tieBeneficiaryB)
        }.OrderBy(row => row.Request.Id).ToArray();
        var secondPage = new PagedResult<ServiceRequestQueueRecord>(2, 2, 4, tiedRows);
        var requestReadStore = new Mock<IServiceRequestReadStore>();
        requestReadStore
            .SetupSequence(store => store.GetQueuePageAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ServiceType?>(), It.IsAny<RequestStatus?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(),
                It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstPage)
            .ReturnsAsync(secondPage);

        var handler = new GetPriorityQueueQueryHandler(
            requestReadStore.Object,
            new FixedTimeProvider(new DateTimeOffset(2025, 3, 15, 0, 0, 0, TimeSpan.Zero)),
            new PriorityCalculator(1_000));

        var page = await handler.HandleAsync(new GetPriorityQueueQuery(1, 2, null, null, null, "priority", "desc"), CancellationToken.None);
        var otherPage = await handler.HandleAsync(new GetPriorityQueueQuery(2, 2, null, null, null, "priority", "desc"), CancellationToken.None);

        Assert.Equal(4, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(highPriorityRequest.Id, page.Items[0].Id);
        Assert.Equal(olderRequest.Id, page.Items[1].Id);
        Assert.Equal(
            new[] { tieRequestA.Id, tieRequestB.Id }.OrderBy(id => id).ToArray(),
            otherPage.Items.Select(item => item.Id).ToArray());
    }

    private static Beneficiary CreateBeneficiary(
        string nationalId,
        int age,
        DisabilityType disabilityType) => new(new CreateBeneficiaryInput(
            nationalId, age, MaritalStatus.Single, 0, disabilityType, 1_000));

    private static ServiceRequest CreateRequest(
        Beneficiary beneficiary,
        ServiceType serviceType,
        DateOnly registrationDate,
        string description) => beneficiary.AddRequest(
            new CreateServiceRequestInput(beneficiary.Id, serviceType, description), registrationDate, 0);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}