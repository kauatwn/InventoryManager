# Inventory Manager

A robust Web API for product inventory management built with **C# 14** and **.NET 10**. This project demonstrates the application of **Clean Architecture**, **DDD Lite**, **RESTful Best Practices**, and a comprehensive testing strategy combining Unit and Integration tests.

## Table of Contents

- [Prerequisites](#prerequisites)
- [How to Run](#how-to-run)
- [Project Structure](#project-structure)
- [Architecture & Design Principles](#architecture--design-principles)

## Prerequisites

Ensure you have the following installed to run this project efficiently:

- **[.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** (or later)
- **[Docker](https://www.docker.com/)** (Optional, for containerization)
- **IDE:** [Visual Studio](https://visualstudio.microsoft.com), [Visual Studio Code](https://code.visualstudio.com/), or [Rider](https://www.jetbrains.com/rider/).

## How to Run

### 1. Clone the Repository

```bash
git clone https://github.com/kauatwn/InventoryManager.git
```

### 2. Enter the Directory

```bash
cd InventoryManager
```

### 3. Choose Execution Method

You can run the application using **Docker** (recommended) or **Locally**.

#### Option A: Run with Docker

1. Build the image:

    ```bash
    docker build -t inventory-manager -f src/InventoryManager.API/Dockerfile .
    ```

2. Run the container:

    ```bash
    docker run --rm -it -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development --name inventory-manager inventory-manager
    ```

*The API documentation will be accessible at `http://localhost:8080/swagger`.*

#### Option B: Run Locally

1. Restore dependencies:

    ```bash
    dotnet restore
    ```

2. Run the API:

    ```bash
    dotnet run --project src/InventoryManager.API
    ```

*The API documentation will be accessible at `http://localhost:5270/swagger`.*

### 4. Execute Tests

To validate the domain logic, application flow, and API contracts:

```bash
dotnet test
```

## Project Structure

The solution follows the **Clean Architecture** principles to ensure separation of concerns and testability, with a dedicated split between Unit and Integration testing.

```plaintext
InventoryManager/
├── src/
│   ├── InventoryManager.API/              # Entry point, Controllers, Swagger
│   ├── InventoryManager.Application/      # Use Cases, DTOs, Mappers
│   ├── InventoryManager.Domain/           # Entities, Value Objects, Interfaces (Pure Logic)
│   └── InventoryManager.Infrastructure/   # EF Core, Repositories, Persistence
└── tests/
    ├── InventoryManager.UnitTests/        # Fast, isolated tests (Domain & Application)
    └── InventoryManager.IntegrationTests/ # Full stack tests (Controller -> Db)
```

## Architecture & Design Principles

This repository prioritizes **software engineering quality** and **maintainability**, following strict development guidelines.

### 1. Domain-Driven Design (DDD Lite)

The core logic resides entirely within the `Domain` layer.

- **Rich Domain Models:** Entities like `Product` enforce invariants (e.g., negative prices or empty names are impossible states).
- **Encapsulation:** Properties use `private set` to ensure changes only happen through valid business methods or constructors.
- **Value Objects:** Concepts like SKU are treated with specific formatting and validation rules.

### 2. Design Patterns

The project utilizes established patterns to ensure modularity and scalability.

| Pattern                  | Usage Scenario                         | Implementation                  |
|:------------------------:|:--------------------------------------:|:--------------------------------|
| **Repository**           | Abstracting data access logic          | `IProductRepository`            |
| **Dependency Injection** | Decoupling layers and enabling testing | `IServiceCollection` extensions |
| **Result / Wrapper**     | Standardizing API responses            | `PagedResponse<T>`              |

### 3. Comprehensive Testing Strategy

The project adopts a "Testing Pyramid" approach using **xUnit** and **Moq**.

- **Unit Tests:** Verify business rules and Use Cases in isolation. Fast execution (<100ms).
- **Integration Tests:** Verify the entire pipeline from the API Controller down to the Database.
  - **Technology:** Uses `WebApplicationFactory` and **EF Core InMemory**.

> [!IMPORTANT]
> **Architectural Decision: Testing Isolation Strategy**
>
> In this project, we use the **EF Core InMemory** provider for simplicity and speed. Since `InMemory` maintains shared state during the test suite execution (due to the use of `IClassFixture` for performance), we adopted a **Defensive Strategy**:
>
> - We use **randomized IDs and SKUs** in write tests. This guarantees that a test never fails due to residual data left by another test (collision).
> - In production scenarios with real databases (SQL Server/Postgres), it is common to use **Total Isolation** strategies, such as **Transaction Rollbacks** or **Database Resets** (using libraries like [TestContainers](https://testcontainers.com/)), which would allow the use of static data.
> - We maintained the defensive approach here to focus didactically on the **API Contract** without the overhead of Docker/Containers configuration.

### 4. CI/CD & Quality

The project includes a **GitHub Actions** workflow that ensures quality on every push:

- **Fail-Fast Pipeline:** Runs Unit Tests first; Integration Tests only run if Unit Tests pass.
- **Static Analysis:** Integrates with **SonarQube Cloud** for code quality gates and coverage reporting.
- **Docker Build:** Verifies that the container image builds successfully.
