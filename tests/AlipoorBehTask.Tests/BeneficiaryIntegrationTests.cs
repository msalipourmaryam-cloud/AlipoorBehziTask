using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Domain;
using AlipoorBehTask.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AlipoorBehTask.Tests;

public sealed class BeneficiaryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public BeneficiaryIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AlipoorBehTaskDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AlipoorBehTaskDbContext>>();
                services.AddDbContext<AlipoorBehTaskDbContext>(options =>
                    options.UseInMemoryDatabase("BeneficiaryIntegrationTests"));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AlipoorBehTaskDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task CreateBeneficiary_ReturnsCreatedRecord()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryInput(
            "5555555555",
            41,
            MaritalStatus.Single,
            1,
            DisabilityType.None,
            600_000m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);
        Assert.NotNull(payload);
        Assert.Equal("5555555555", payload!.NationalId);
    }

    [Fact]
    public async Task UpdateBeneficiary_ChangesExistingValues()
    {
        using var client = _factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryInput(
            "6666666666",
            30,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            300_000m));
        var createdBody = await created.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);

        var updated = await client.PutAsJsonAsync($"/api/beneficiaries/{createdBody!.Id}", new CreateBeneficiaryInput(
            "6666666666",
            31,
            MaritalStatus.Married,
            2,
            DisabilityType.Moderate,
            500_000m));

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var payload = await updated.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);
        Assert.Equal(31, payload!.Age);
        Assert.Equal(MaritalStatus.Married, payload.MaritalStatus);
        Assert.Equal(2, payload.DependentCount);
    }

    [Fact]
    public async Task DeleteBeneficiary_RemovesRecord()
    {
        using var client = _factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryInput(
            "7777777777",
            44,
            MaritalStatus.Widowed,
            1,
            DisabilityType.Severe,
            1_200_000m));
        var beneficiary = await created.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);

        var response = await client.DeleteAsync($"/api/beneficiaries/{beneficiary!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await client.GetAsync($"/api/beneficiaries/{beneficiary.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
