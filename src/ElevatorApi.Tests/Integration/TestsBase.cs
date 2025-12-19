using System.Net;
using System.Net.Http.Json;
using ElevatorApi.Api.Config;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Tests.Integration;

internal abstract class TestsBase
{
    private WebApplicationFactory<Program> Factory { get; set; }
    protected HttpClient Client { get; set; }
    protected abstract ElevatorSettings ElevatorSettings { get; }


    [SetUp]
    public void Setup()
    {
#pragma warning disable CA2000 // factory is disposed in TearDown()

        var factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(Options.Create(ElevatorSettings));
                });
            });


        Factory = factory;
        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    [TearDown]
    public void TearDown()
    {
        Client.Dispose();
        Factory?.Dispose();
    }

    #region Helpers

    private async Task<HttpResponseMessage> Get(string endpoint)
    {
        return await Client.GetAsync(new Uri(endpoint));
    }

    private async Task<HttpResponseMessage> Post(string endpoint)
    {
        return await Client.PostAsync(new Uri(endpoint), null);
    }

    protected async Task<HttpResponseMessage> GetCarResponse(byte carId)
        => await Get($"/cars/{carId}");

    protected async Task<HttpResponseMessage> MoveCarResponse(byte carId)
        => await Post($"/cars/{carId}/move");

    protected async Task<HttpResponseMessage> AddCarStopResponse(byte carId, sbyte floorNumber)
        => await Post($"/cars/{carId}/stops/{floorNumber}");

    protected async Task<HttpResponseMessage> CallCarResponse(sbyte floorNumber)
        => await Post($"/cars/call/{floorNumber}");

    /// <summary>
    /// Parses a CarModel instance from the provided HttpResponseMessage
    /// </summary>
    /// <param name="response">The response with content to parse</param>
    /// <exception cref="InvalidOperationException">Thrown when response contains unparseable content</exception>
    protected static async Task<CarResponse> ParseCar(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CarResponse>()
               ?? throw new InvalidOperationException("Response was null");
    }

    #endregion
}