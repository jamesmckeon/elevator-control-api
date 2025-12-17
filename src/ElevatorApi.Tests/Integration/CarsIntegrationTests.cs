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

    private const string baseUrl = "/cars";

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

    #region Index

    [Test]
    public async Task Index_CarExists_Returns200()
    {
        byte carId = 1;

        var response = await Get($"{baseUrl}/{carId}");
        var car = await response.Content.ReadFromJsonAsync<CarResponse>();

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
        var response = await Get($"{baseUrl}/{carId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region AddStop

    [Test]
    public async Task AddStop_CarExists_Returns200()
    {
        var carId = 1;
        var floorNumber = ElevatorSettings.MinFloor;

        var url = $"{baseUrl}/{carId}/Stops/{floorNumber}";
        var response = await Post(url);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        var car = await response.Content
            .ReadFromJsonAsync<CarResponse>();

        Assert.That(car, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(car.Id, Is.EqualTo(carId));
            Assert.That(car.Stops, Does.Contain(floorNumber));
            Assert.That(car.NextFloor, Is.EqualTo(floorNumber));
        });
    }

    [Test]
    public async Task AddStop_InvalidCarId_Returns404()
    {
        var carId = ElevatorSettings.CarCount + 1;
        var floorNumber = ElevatorSettings.MinFloor;

        var url = $"{baseUrl}/{carId}/Stops/{floorNumber}";
        var response = await Post(url);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region MoveCar

    [Test]
    public async Task MoveCar_CarExists_Returns200()
    {
        var carId = 1;
        var floorNumber = ElevatorSettings.MinFloor;

        var stopsUrl = $"{baseUrl}/{carId}/Stops/{floorNumber}";
        var stopsResponse = await Post(stopsUrl);
        var car = await stopsResponse.Content
            .ReadFromJsonAsync<CarResponse>();

        Assert.That(car, Is.Not.Null);
        var nextFloor = car.NextFloor;

        var moveUrl = $"{baseUrl}/{carId}/Move";
        var moveResponse = await Post(moveUrl);

        Assert.That(moveResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));
        var movedCar = await moveResponse.Content
            .ReadFromJsonAsync<CarResponse>();
        Assert.That(movedCar, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(movedCar.Id, Is.EqualTo(carId));
            Assert.That(movedCar.CurrentFloor, Is.EqualTo(nextFloor));
            Assert.That(movedCar.Stops, Is.Empty);
            Assert.That(movedCar.NextFloor, Is.Null);
        });
    }

    [Test]
    public async Task MoveCar_InvalidCarId_Returns404()
    {
        var carId = ElevatorSettings.CarCount + 1;

        var url = $"{baseUrl}/{carId}/Move";
        var response = await Post(url);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region CallCar

    [Test]
    public async Task CallCar_ValidFloor_Returns200()
    {
        var floorNumber = ElevatorSettings.MinFloor;

        var url = $"{baseUrl}/call/{floorNumber}";
        var response = await Post(url);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        var car = await response.Content.ReadFromJsonAsync<CarResponse>();

        Assert.That(car, Is.Not.Null);
        Assert.That(car.NextFloor, Is.EqualTo(floorNumber));
    }

    [Test]
    public async Task CallCar_InvalidFloorNumber_Returns400()
    {
        var url = $"{baseUrl}/call/{ElevatorSettings.MinFloor - 1}";
        var response = await Post(url);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion

    #region Helpers

    private async Task<HttpResponseMessage> Get(string endpoint)
    {
        return await Client.GetAsync(new Uri(endpoint));
    }

    private async Task<HttpResponseMessage> Post(string endpoint)
    {
        return await Client.PostAsync(new Uri(endpoint), null);
    }

    #endregion
}