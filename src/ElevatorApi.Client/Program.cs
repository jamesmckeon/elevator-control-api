// displays message and prompts user to exit the program

// ReSharper disable All

using System.Net.Http.Json;
using System.Text.Json;
using ElevatorApi.Api.Models;

void Exit(string message)
{
    Console.WriteLine(message);
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    // throw new ApplicationException("Application terminated");
}

try
{
    #region Setup

    using var handler = new HttpClientHandler()
    {
        CheckCertificateRevocationList = true
    };

    using var client = new HttpClient(handler);
    client.BaseAddress = new Uri("http://localhost:8080");

    // executes a GET request and returns the response if successful
    var ping = async (string endpoint) =>
    {
        var response = await client.GetAsync(new Uri(endpoint, UriKind.Relative));

        if (!response.IsSuccessStatusCode)
        {
            Exit($"Error: {endpoint} endpoint returned {response.StatusCode}.");
        }

        return response;
    };

    // executes a request and returns the corresponding CarResponse DTO if successful
    var pingCar = async (string endpoint, HttpMethod method) =>
    {
        HttpResponseMessage response;

        if (method == HttpMethod.Get)
        {
            response = await client.GetAsync(new Uri(endpoint, UriKind.Relative));
        }
        else if (method == HttpMethod.Post)
        {
            response = await client.PostAsync(new Uri(endpoint, UriKind.Relative), null);
        }
        else
        {
            throw new NotSupportedException($"Method {method} not supported");
        }

        if (!response.IsSuccessStatusCode)
        {
            Exit($"Error: {endpoint} endpoint returned {response.StatusCode}.");
        }

        var car = await response.Content.ReadFromJsonAsync<CarResponse>();
        if (car == null)
        {
            Exit($"Error: {endpoint} returned null response.");
        }

        Console.WriteLine($"Response: {JsonSerializer.Serialize(car)}");
        return car!;
    };

    var moveCar = async (CarResponse carToMove) =>
    {
        Console.WriteLine($"Moving car #{carToMove.Id} to next stop ...");

        var response = await client.PostAsync(new Uri($"/cars/{carToMove.Id}/move", UriKind.Relative), null);

        if (!response.IsSuccessStatusCode)
        {
            Exit($"Error: move endpoint returned {response.StatusCode}.");
        }

        var movedCar = await response.Content.ReadFromJsonAsync<CarResponse>();
        if (movedCar == null)
        {
            Exit($"Error: move endpoint returned null response.");
        }

        Console.WriteLine($"Response: {JsonSerializer.Serialize(movedCar)}");
        Console.WriteLine("");

        return movedCar!;
    };

    #endregion

    Console.WriteLine("Checking health ...");
    await ping("/health");
    Console.WriteLine();

    Console.WriteLine("Person on floor 1 calls elevator ...");
    var car1 = await pingCar("/cars/call/1", HttpMethod.Post);
    Console.WriteLine();

    Console.WriteLine($"Retrieving car #{car1.Id} status ...");
    car1 = await pingCar($"/cars/{car1.Id}", HttpMethod.Get);
    Console.WriteLine();

    await moveCar(car1);

    Console.WriteLine($"Person in car #{car1.Id} presses button for floor 4 ...");
    await pingCar($"/cars/{car1.Id}/stops/4", HttpMethod.Post);
    Console.WriteLine();

    Console.WriteLine($"Checking car #{car1.Id} destinations ...");
    await pingCar($"/cars/{car1.Id}", HttpMethod.Get);
    Console.WriteLine();

    car1 = await moveCar(car1);

    Console.WriteLine("Person on floor -1 calls elevator ...");
    var car2 = await pingCar("/cars/call/-1", HttpMethod.Post);
    Console.WriteLine();

    Console.WriteLine($"Checking car #{car2.Id} status ...");
    await pingCar($"/cars/{car2.Id}", HttpMethod.Get);
    Console.WriteLine();

    await moveCar(car2);

    Console.WriteLine("Demo complete. Press any key to exit...");
    Console.ReadKey();
}
catch (HttpRequestException ex)
{
    Exit($"Request encountered an exception ('{ex.Message}'): verify that api is running.");
}
#pragma warning disable CA1031
catch (Exception ex)
#pragma warning restore CA1031

{
    Exit($"Exception encountered: {ex.Message}");
}