# Inventory Management API

![CI](https://github.com/qvpnguyen/Inventory/actions/workflows/ci.yml/badge.svg)

**Live demo:** [Swagger UI](https://inventory-api-qvpnguyen-cecpf2fdg5e2fbe7.canadacentral-01.azurewebsites.net/swagger)

A clean, test-driven ASP.NET Core Web API for managing products and orders.
This project focuses on backend correctness, clean architecture, and real-world patterns
rather than client-side implementation.

---

## Features

- Product management (CRUD)
- Order creation with stock validation
- Transactional rollback on failure
- Real-time notifications with SignalR
- Global exception handling middleware
- DTO-based API contracts
- Ownership and authorization checks
- JWT authentication
- Unit testing with in-memory SQLite

---

## Architecture Overview

The project follows a layered / clean architecture approach:

- Controllers handle HTTP concerns only
- Services contain all business logic
- Domain entities represent core business models
- DTOs define API input and output
- EF Core manages persistence
- SignalR handles real-time notifications

Core principles applied:
- Separation of Concerns
- Dependency Injection

---

## Project Structure

### Inventory.Api

```
Inventory.Api
├── Controllers
│ ├── AuthController.cs
│ ├── ProductsController.cs
│ └── OrdersController.cs
│
├── Services
│ ├── Interfaces
│ │ ├── IAuthService.cs
│ │ ├── IProductService.cs
│ │ └── IOrderService.cs
│ ├── AuthService.cs
│ ├── ProductService.cs
│ └── OrderService.cs
│
├── Domain
│ └── Entities
│ ├── User.cs
│ ├── Product.cs
│ ├── Order.cs
│ └── OrderItem.cs
│
├── DTOs
│ ├── Orders
│ │ ├── CreateOrderRequest.cs
│ │ ├── CreateOrderItemRequest.cs
│ │ ├── OrderResponse.cs
│ │ └── OrderItemResponse.cs
│ └── Products
│   ├── CreateProductRequest.cs
│   ├── UpdateProductRequest.cs
│   └── ProductResponse.cs
│
├── Hubs
│ └── OrderHub.cs
│
├── Middleware
│ └── ExceptionHandlingMiddleware.cs
│
├── Exceptions
│ ├── NotFoundException.cs
│ ├── ForbiddenException.cs
│ └── BadRequestException.cs
│
├── Persistence
│ └── AppDbContext.cs
│
└── Program.cs
```

### Inventory.Tests

```
Inventory.Tests
├── Helpers
│ ├── DbContextFactory.cs
│ └── SqliteTestDatabase.cs
│
├── Hubs
│ └── OrderHubTests.cs
│
└── Services
  └── OrderServiceTests.cs
```

---

## Domain Entities

Domain entities represent the core business logic and rules.
They are never exposed directly through the API.

Main entities:
- User
- Product
- Order
- OrderItem

---

## DTOs (Data Transfer Objects)

DTOs define what the API exposes to clients.

### Orders DTOs
- CreateOrderRequest
- CreateOrderItemRequest
- OrderResponse
- OrderItemResponse

### Products DTOs
- CreateProductRequest
- UpdateProductRequest
- ProductResponse

All input DTOs use Data Annotations for validation.

---

## Mapping

Entity-to-DTO mappings are found in ProductService.cs and OrderService.cs using the MapToResponse method.

This ensures controllers never expose domain entities directly.

---

## Security and Authorization

- Users can only access their own products and orders
- Ownership checks are enforced in services
- Forbidden access throws ForbiddenException
- Missing resources throw NotFoundException

This prevents information leakage and enforces business rules.

---

## SignalR

SignalR is used to broadcast order-related events.

- OrderHub is injected via IHubContext
- No client implementation is required
- In tests, SignalR is fully mocked

---

## Testing Strategy

- xUnit for unit testing
- In-memory SQLite for tests involving persistence and transactions
- EF Core InMemory for tests that don't touch the database (SignalR hub)
- SignalR mocked via IHubContext

SQLite is used rather than the EF Core InMemory provider because InMemory silently
ignores transactions and does not enforce schema constraints — rollback tests would
pass without exercising any rollback. In-memory SQLite behaves like a real relational
database while keeping tests fast and free of external dependencies.

---

## Deployment

The API is deployed to Azure App Service with a managed PostgreSQL database.
Build, tests, and deployment run automatically on every push to `main` via
GitHub Actions.

Note: the free tier sleeps after inactivity — the first request may take up to
a minute to respond.

---

## Exception Handling

All exceptions are handled globally via middleware.

Controllers do not contain try/catch blocks.

Custom exceptions:
- NotFoundException
- ForbiddenException
- BadRequestException

Each exception maps to a proper HTTP status code.

---

## Project Phases

1. API setup and structure
2. Product CRUD
3. Order creation
4. Transactions and rollback
5. SignalR integration
6. Validation and exception handling
7. JWT authentication
8. Authorization and ownership checks
9. Documentation and cleanup
10. CI/CD pipeline and Azure deployment

---

## Configuration

Secrets are not versioned. To run locally, create `Inventory.Api/appsettings.Development.json`:

​```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=inventory;Username=...;Password=..."
  },
  "JwtSettings": {
    "SecretKey": "a-key-of-at-least-32-bytes"
  }
}
​```

Then apply migrations with `dotnet ef database update`.

---

## Notes

- No frontend or client is implemented
- The project is API-first and backend-focused
- Designed as a portfolio-quality backend project

---

## License

This project is intended for educational and portfolio purposes.

---

## Author

Patrick Nguyen