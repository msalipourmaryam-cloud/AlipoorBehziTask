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

public sealed class BeneficiaryE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program> _factory;

    public BeneficiaryE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AlipoorBehTaskDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AlipoorBehTaskDbContext>>();
                services.AddDbContext<AlipoorBehTaskDbContext>(options =>
                    options.UseInMemoryDatabase("BeneficiaryE2ETests"));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AlipoorBehTaskDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        });
    }

    [Fact]
    public async Task EndToEnd_BeneficiaryLifecycle_WorksAcrossApiCalls()
    {
        using var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/beneficiaries", new CreateBeneficiaryInput(
            "8888888888",
            26,
            MaritalStatus.Single,
            0,
            DisabilityType.None,
            0m));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);

        var get = await client.GetAsync($"/api/beneficiaries/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var fetched = await get.Content.ReadFromJsonAsync<BeneficiaryResponse>(JsonOptions);
        Assert.Equal(created.Id, fetched!.Id);

        var update = await client.PutAsJsonAsync($"/api/beneficiaries/{created.Id}", new CreateBeneficiaryInput(
            "8888888888",
            28,
            MaritalStatus.Married,
            1,
            DisabilityType.Moderate,
            20_000m));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/beneficiaries/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalGet = await client.GetAsync($"/api/beneficiaries/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, finalGet.StatusCode);
    }
}
