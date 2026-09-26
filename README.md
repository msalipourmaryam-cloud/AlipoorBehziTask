# Alipoor BehTask

This project is a .NET 10 service-management application for registering beneficiaries, creating service requests, prioritizing them, and exposing dashboard/report summaries. The solution was built around a clean layered architecture so that business rules remain independent from HTTP concerns, persistence, and UI behavior.

## Why this architecture was chosen

The application uses a layered, domain-driven structure:

- Domain layer: holds the business rules, aggregate logic, domain events, validation, and score calculation.
- Application layer: contains commands, queries, handlers, and orchestration logic.
- Infrastructure layer: implements persistence, repositories, EF Core mapping, and database setup.
- API layer: exposes HTTP endpoints and keeps controllers thin.

This is a good fit for the project because:

- Business rules stay centralized and testable.
- The logic is not coupled to ASP.NET Core or EF Core.
- Changes in persistence or API shape do not force business rules to change.
- It is easier to add unit, integration, and end-to-end tests around the real behavior.
- The code is easier to extend when new service types, statuses, or reports are introduced.

## Stack and technology choices

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit + WebApplicationFactory
- Swagger / OpenAPI
- HTML/CSS/JavaScript dashboard front-end served by the API project

These technologies were selected because they provide a straightforward full-stack backend solution with robust validation, persistence, and local developer workflow support. The stack is simple to run, easy to test, and well suited to a medium-sized internal operational workflow.

## Solution structure

- `src/AlipoorBehTask.Domain`
  - aggregates and business rules
  - domain events and validation logic
  - repository contracts and core abstractions
- `src/AlipoorBehTask.Application`
  - commands, queries, handlers, and business workflows
  - request priority logic and reports orchestration
- `src/AlipoorBehTask.Infrastructure`
  - EF Core DbContext and entity configuration
  - SQL Server repositories and database extensions
  - data seeding and persistence integration
- `src/AlipoorBehTask.Api`
  - controllers, API endpoints, middleware, dashboard views, and static assets
- `tests/AlipoorBehTask.Tests`
  - domain tests
  - integration tests
  - end-to-end tests
  - edge-case tests

## Functional requirements and implementation status

| Requirement area | Status | Notes |
|---|---|---|
| Beneficiary registration | Implemented | Accepts and validates beneficiary data with duplicate national ID protection. |
| Beneficiary read/update/delete | Implemented | Supported in API and application handlers. |
| Service request creation | Implemented | Validates beneficiary, service type, description, and status flow. |
| Priority calculation | Implemented | Scores are computed in the domain layer with explainable factors. |
| Queue ordering and pagination | Implemented | Supports sorting, paging, filtering, and deterministic scoring order. |
| Request status rules | Implemented | Controlled transitions and terminal states are enforced. |
| Summary reporting | Implemented | Includes service type and status aggregation. |
| Dashboard drill-down | Implemented | Bar/chart views can drill down by service type and status details. |
| CRUD UI for beneficiaries | Implemented | Add, edit, and delete flows are present in the dashboard. |
| Swagger/OpenAPI docs | Implemented | Available in Development. |
| Authentication and authorization | Missing | Not required for the current scope but important for production deployment. |
| Notification system | Missing | SMS/email notifications can be added later. |
| Document uploads | Missing | Not part of the current requirement set. |
| Payment integration | Missing | Not included in the original scope. |
| External system integration | Missing | Could be added as a later integration layer. |
| Containerization / CI pipeline | Missing | Useful for deployment automation and repeatable environment setup. |
| Advanced analytics dashboards | Missing | Can be added as future reporting improvements. |

## Non-functional requirements

The application is designed to satisfy the core non-functional expectations of the task:

- Business rules are separated from infrastructure and HTTP logic.
- Validation and domain rules are enforced centrally.
- API responses are predictable and consistent.
- Business behavior is covered by automated tests.
- Data is stored with stable schema and relational constraints.
- OpenAPI docs are exposed for easier integration and testing.
- The codebase is modular enough to evolve without major refactors.

## Technical requirements

To build and run the project locally, the following is required:

- .NET 10 SDK
- Windows or Linux/macOS development machine
- SQL Server instance available locally or a connection string configured for an alternate server
- Optional: access to a SQL Server database and local dev environment for the API

## How to build the app

From the solution root:

```powershell
dotnet restore

dotnet build "src/AlipoorBehTask.Api/AlipoorBehTask.Api.csproj"
```

## How to run the app

```powershell
dotnet run --project "src/AlipoorBehTask.Api/AlipoorBehTask.Api.csproj" --urls http://localhost:5080
```

Then open:

- http://localhost:5080
- http://localhost:5080/swagger (Development)

## How to test the app

Run the full test suite:

```powershell
dotnet test "tests/AlipoorBehTask.Tests/AlipoorBehTask.Tests.csproj"
```

## Tests covered

The project includes the following test types:

- Unit tests for domain validation and score logic
- Integration tests for API endpoints and persistence flow
- End-to-end tests for the beneficiary lifecycle across the public API
- Edge-case tests for invalid enum values, negative income, invalid data boundaries, and status validation behavior

This gives confidence in both business rules and endpoint behavior, while also protecting against regressions in high-risk scenarios.

## Configuration and assumptions

- The application uses a configurable threshold for poverty income comparison.
- Service requests follow a status lifecycle that allows only valid transitions.
- Registration dates are assigned on the server side and stored consistently.
- The default database is expected to be a local SQL Server instance unless overridden by `ConnectionStrings__DefaultConnection`.
- Development seeding is available for faster local exploration and testing; production should not rely on auto-seeding.

## Benefits of the current implementation

- Clear separation of concerns.
- Strong validation at the domain boundary.
- Better maintainability and safer future changes.
- Better testability than mixing rules, persistence, and HTTP logic together.
- Foundation for adding new reporting or business features without rewriting existing flows.

## Future improvements that can be added later

- Authentication and authorization
- Role-based access control
- Email/SMS notifications
- Document upload and management
- Audit history and reporting exports
- CI/CD pipeline and Docker support
- Better UX refinements and analytics screens

## Summary

The application is a working, test-backed service for beneficiary and service-request management with layered architecture, robust domain rules, priority scoring, queue logic, and reporting. It covers the core assignment requirements and leaves room for production hardening and expansion in a clean, maintainable way.