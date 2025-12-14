using System.Net;
using System.Net.Http.Json;
using ElevatorApi.Api.Config;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Tests.Integration;

[Category("Integration")]
public class CarsIntegrationTests
{
    private WebApplicationFactory<Program> Factory { get; set; }
    private HttpClient Client { get; set; }
    private ElevatorSettings ElevatorSettings { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        ElevatorSettings = new ElevatorSettings
        {
            CarCount = 3,
            MinFloor = -2,
            MaxFloor = 10
        };

        // Factory is disposed in OneTimeTearDown
#pragma warning disable CA2000

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(Options.Create(ElevatorSettings));
                });
            });
#pragma warning restore CA2000

        Factory = factory;
    }

    [SetUp]
    public void Setup()
    {
        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Factory?.Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        Client.Dispose();
    }

    [Test]
    public async Task Index_CarExists_Returns200()
    {
        byte carId = 1;

        var response = await Get($"/api/cars/{carId}");
        var car = await response.Content.ReadFromJsonAsync<Car>();

        Assert.That(car, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(car.Id, Is.EqualTo(carId));
        });
    }

    [Test]
    public async Task Index_CarDoesntExist_ReturnsNotFound()
    {
        var carId = ElevatorSettings.CarCount + 1;
        var response = await Get($"/api/cars/{carId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private async Task<HttpResponseMessage> Get(string endpoint)
    {
        return await Client.GetAsync(new Uri(endpoint));
    }
}