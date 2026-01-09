# Elevator Control System API

An open-source RESTful elevator control system API for testing client applications and service-to-service integration.

> **Origin**: This project was originally developed as a technical assignment for a job application and has been released as an open-source testing tool.

## Overview

This API provides a fully functional elevator control system that you can run locally to test:
- Client applications that interact with elevator systems
- Service-to-service integration patterns
- RESTful API consumption
- Real-time state management scenarios
- Complex assignment algorithms

**Key Design Decisions for Testing Use:**
- **In-memory storage** - No database setup required, runs immediately with `dotnet run`
- **Zero external dependencies** - No Redis, message queues, or cloud services needed
- **Configurable** - Adjust car count and floor range via `appsettings.json`
- **Stateless** - Resets on restart, ensuring clean test runs
- **Comprehensive API** - Supports both direct car control and intelligent car assignment

## Intentional Scope Limitations

This API focuses on core elevator control logic and testing utility. It **intentionally excludes**:

- **Database persistence** - In-memory storage keeps setup simple; production systems would use SQL Server/PostgreSQL
- **Authentication** - No API keys or JWT tokens required for ease of testing
- **Cloud deployment** - Runs locally; Docker/Kubernetes configs not included
- **Advanced middleware** - No rate limiting, caching, or API versioning
- **Logging infrastructure** - Basic `ILogger` only

These exclusions make the API lightweight and immediately usable for testing purposes.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Quick Start

Clone the repository and run with a single command:

```bash
cd src/ElevatorApi.Api
dotnet run
```

The API will be available at `http://localhost:8080`

## Configuration

The elevator system can be configured in `src/ElevatorApi.Api/appsettings.json`.  The default configuration is listed below:

```json
{
  "ElevatorSettings": {
    "CarCount": 2,         // Number of elevator cars in the system
    "MinFloor": -1,        // Minimum valid floor number
    "MaxFloor": 5,         // Maximum valid floor number
    "lobbyFloor": 0        // The start floor for all cars
  }
}
```

## API Endpoints

- endpoint details can be browsed via the swagger ui available at [http://localhost:8080/swagger]
- api health can be checked via the /health endpoint

### CarResponse Object

All endpoints return a `CarResponse` object with the following structure:

```json
{
  "id": 1,           // Unique identifier for the elevator car
  "currentFloor": 0,    // Current floor position of the car
  "nextFloor": null,    // Next scheduled floor (null if no stops)
  "stops": []           // All stops assigned to the car, in order
}
```

### Get Car State

```bash
# Get current state of car #1
curl http://localhost:8080/cars/1
```

**Response Codes:**

- `200 OK` - Car state retrieved successfully
- `404 Not Found` - Invalid car ID

### Request Pickup

```bash
# Request an elevator to pick up at floor 5
curl -X POST "http://localhost:8080/cars/call/5"
```

**Response Codes:**

- `200 OK` - Pickup request processed successfully
- `400 Bad Request` - Invalid floor number

### Add Stop to Car

```bash
# Add a stop at floor 4 to car 1
curl -X POST http://localhost:8080/cars/1/stops/4
```

**Response Codes:**

- `200 OK` - Stop added successfully
- `400 Bad Request` - Invalid floor number
- `404 Not Found` - Invalid car ID

### Advance Car

```bash
# Move car 1 to its next stop
curl -X POST http://localhost:8080/cars/1/move
```

**Response Codes:**

- `200 OK` - Car advanced successfully
- `404 Not Found` - Invalid car ID

## Demo Client

A demo client application is included to demonstrate API usage and test the elevator system.

### Running the Demo Client

Make sure the API is running first (see [Quick Start](#quick-start)), then in a separate terminal:

```bash
cd src/ElevatorApi.Client
dotnet run
```

The demo client will connect to the API at `http://localhost:8080` and demonstrate various elevator operations.

## Testing

Run tests:

```bash
cd src/ElevatorApi.Tests
dotnet test
```

Run by category:

```bash
  dotnet test --filter Category=Unit
  dotnet test --filter Category=Integration
```

## Car Assignment Rules

The system uses the following rules when assigning a car to a requested pickup floor, towards minimizing passenger wait time.

### Assignment Priority

#### 1. Idle Cars First

Idle cars (cars with no assigned stops) are always assigned first when available. If multiple idle cars exist, the closest one is selected.

#### 2. Busy Car Selection

When no idle cars are available, the system selects the optimal car using these criteria:

1. **Primary criterion**: Car with fewest stops before reaching the called floor
2. **Tiebreaker**: Car whose nearest existing stop is closest to the called floor
3. **Rationale**: Door opening/closing cycles impact wait time more than travel distance

### Example Scenario

When a pickup is requested at floor 5:

- **Car 1**: `stops [1, 2]` → 2 stops before floor 5, nearest stop is 2 (3 floors away)
- **Car 2**: `stops [12, 9, 7]` → 3 stops before floor 5, nearest stop is 7 (2 floors away)
- **Car 3**: `stops [10, 7]` → 2 stops before floor 5, nearest stop is 7 (2 floors away) ✓

**Result**: Car 3 is assigned (tied with Car 1 on stop count at 2 stops, but Car 3's nearest stop at floor 7 is closer to floor 5 than Car 1's nearest stop at floor 2)

### Idle Behavior

Cars remain stationary at their current floor when idle, rather than returning to a home/lobby position.

## Notes
- App data is stored in memory only, i.e., it resets on application restart
