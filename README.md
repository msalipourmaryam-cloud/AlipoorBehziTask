# Alipoor beneficiary services API

This .NET 10 solution implements the Persian assignment in `Mid-Level_Test.pdf`. It follows the separation of domain rules, application commands/queries, repositories, persistence, and thin HTTP controllers.

## Structure

The solution follows project boundaries and folder conventions:

- `src/AlipoorBehTask.Domain/Aggregates`: `IBeneficiary` is the aggregate-root contract; `Beneficiary` owns its `ServiceRequest` children and controls request status changes.
- `src/AlipoorBehTask.Domain/DTOs`: registration DTO contracts consumed by the aggregate.
- `src/AlipoorBehTask.Domain/Events`: typed domain events collected by the aggregate.
- `src/AlipoorBehTask.Domain/IRepositories`: aggregate and event-store repository contracts.
- `src/AlipoorBehTask.Domain/Tools`: entity/domain-event base contracts, repository, and unit-of-work abstractions.
- `src/AlipoorBehTask.Application/Commands`, `Handlers`, `Queries`, and `Tools`: explicit write commands, command handlers, read queries, query handlers, event mediator, and handler contracts.
- `src/AlipoorBehTask.Infrastructure/Models`, `Repositories`, `Extensions`, and `Migrations`: EF Core SQL Server mapping, aggregate and event-log repositories, database registration, and schema history.
- `src/AlipoorBehTask.Api/Controllers` and `Handlers`: thin HTTP endpoints and application event handlers.
- `tests/AlipoorBehTask.Tests`: focused domain and application unit tests.

Service requests are not independent aggregate roots. Creating or changing a request loads and mutates its Beneficiary aggregate; the aggregate enforces the rule and collects its events. Application command handlers dispatch those events through the mediator, event handlers add audit records to the same EF unit of work, and the aggregate plus event log commit together.

## Implemented behaviors

- Beneficiary registration rejects invalid fields and duplicate normalized national IDs.
- Service requests start in `Pending` and use controlled service types: `Wheelchair`, `HousingDepositLoan`, and `Pension`.
- The initial priority score is calculated in the Domain layer and stored when the request is registered. The response also includes the current score, recalculated with waiting time, and a factor-by-factor explanation. Waiting time uses completed calendar months between registration and the current UTC date.
- The queue orders by score descending, registration date ascending, and request ID ascending, with service/status filtering and bounded pagination.
- Status transitions are controlled: `Pending` can become `Approved` or `Rejected`; `Approved` can become `Completed`. `Rejected` and `Completed` are terminal.
- The summary includes every service-type/status combination, including zero counts.
- Beneficiary and service-request tables use server-side filtering, sorting, counting, and pagination. Request priority sorting is computed in the SQL query before page retrieval.
- The reports view includes a status bar chart and service-by-status count table.

## Configuration and assumptions

The assignment does not give a numeric poverty threshold or a currency/unit. `PriorityScoring:PovertyIncomeThreshold` is therefore configurable and currently has the provisional value `10000000`; replace it with the threshold and unit approved by the organization. The same unit must be used for beneficiary monthly income.

The PDF lists service examples but does not define the transition matrix or waiting-month rounding. This implementation treats each status change as one-way as described above, and counts only completed calendar months. Registration dates are assigned by the server in UTC. The API accepts numeric income with no currency conversion.

The default connection uses Windows Integrated Authentication against the local SQL Server default instance (`localhost`, database `AlipoorBehTask`). Override `ConnectionStrings__DefaultConnection` for another SQL Server. Schema changes are tracked by EF Core migrations; create and apply later migrations with:

```powershell
dotnet ef migrations add MigrationName --project AlipoorBehTask/src/AlipoorBehTask.Infrastructure --startup-project AlipoorBehTask/src/AlipoorBehTask.Api --output-dir Migrations
dotnet ef database update --project AlipoorBehTask/src/AlipoorBehTask.Infrastructure --startup-project AlipoorBehTask/src/AlipoorBehTask.Api
```

## API

- `POST /api/beneficiaries`
- `GET /api/beneficiaries/{id}`
- `GET /api/beneficiaries?page=1&pageSize=20&search=9000000000&sortBy=nationalId&sortDirection=asc`
- `POST /api/service-requests`
- `GET /api/service-requests/{id}`
- `GET /api/service-requests/queue?page=1&pageSize=20&serviceType=Wheelchair&status=Pending&search=9000000000&sortBy=priority&sortDirection=desc`
- `PATCH /api/service-requests/{id}/status`
- `GET /api/reports/requests-summary`

OpenAPI/Swagger is exposed at `/swagger` in Development. SQL Server schema changes are applied from `Infrastructure/Migrations` at startup. In Development, an empty database receives 50 deterministic synthetic beneficiaries and multiple requests per person across every status, so paging and reports have sample content. Production databases are never auto-seeded. No authentication, notifications, document uploads, payments, or external integrations are included, consistent with the assignment scope.