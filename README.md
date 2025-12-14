# Elevator Control System API

A RESTful API for managing elevator control systems, designed to provide integration testing capabilities for dependent teams.

## Prerequisites

- .NET 9.0 SDK

## Quick Start

Clone the repository and run with a single command:

```bash
cd src/ElevatorApi.Api
dotnet run
```

The API will be available at `http://localhost:8080`

## Configuration

The elevator system can be configured in `src/ElevatorApi.Api/appsettings.json`:

```json
{
  "ElevatorSettings": {
    "MinFloor": -2,        // Minimum valid floor number (supports basements)
    "MaxFloor": 50,        // Maximum valid floor number
    "NumberOfCars": 3      // Number of elevator cars in the system
  }
}
```

Modify these values to test different building configurations without recompiling.

## API Endpoints

### Get Car State

```bash
# Get current state of a specific car
curl http://localhost:8080/api/cars/1
```

**Response:**

```json
{
  "carId": 1,
  "currentFloor": 0,
  "direction": "idle",
  "nextFloor": null,
  "origins": [],
  "destinations": []
}
```

### Request Pickup

```bash
# Request an elevator to pick up at floor 5
curl -X PUT "http://localhost:8080/api/pickup-requests?floorNumber=5"
```

### Add Destination

```bash
# Add destination floor 10 to car 1
curl -X PUT http://localhost:8080/api/cars/1/destinations/10
```

### Advance Car

```bash
# Move car 1 to its next floor
curl -X POST http://localhost:8080/api/cars/1/advance
```

## Testing Strategy

### Unit Tests

- **Services**: Business logic and elevator algorithms
- **Repositories**: Data access and car management
- **Validators**: Configuration and input validation

### Integration Tests

- **Full API workflows**: End-to-end request/response testing using WebApplicationFactory
- **Controller coverage**: Achieved through integration tests (controllers are thin pass-through layers)

Run tests:
```bash
cd src/ElevatorApi.Tests
dotnet test
```

## Project Structure

```
src/
├── ElevatorApi.Api/
│   ├── Controllers/      # API endpoints
│   ├── Services/         # Business logic
│   ├── Dal/              # Data access (repositories)
│   ├── Models/           # Domain models
│   └── Config/           # Configuration classes
└── ElevatorApi.Tests/
    ├── Controllers/      # Integration tests
    ├── Services/         # Unit tests for services
    ├── Dal/              # Unit tests for repositories
    └── Config/           # Unit tests for configuration
```

## Architecture Decisions

- **In-memory storage**: Using `ConcurrentDictionary` for thread-safe car state management
- **No async/await**: All operations are synchronous (in-memory only, no I/O)
- **Configuration-based**: Building parameters configurable for testing different scenarios