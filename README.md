# Inventory Management API

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
- Unit testing with EF Core InMemory

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
- SOLID principles
- Dependency Injection
- Persistence Ignorance

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
│ └── DbContextFactory.cs
│
├── Hubs
│ └── OrderHubTests.cs
│
└── Services.cs
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
- EF Core InMemory for isolation
- Transaction rollback verification
- SignalR mocking

EF Core InMemory is used to ensure fast, deterministic tests without external dependencies.

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
7. Authorization and ownership checks
8. Documentation and cleanup

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