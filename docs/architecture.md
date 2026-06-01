# FinTrack Architecture

FinTrack starts with a layered backend architecture:

- `FinTrack.Domain`: entities and enums. This project has no dependency on other layers.
- `FinTrack.Application`: DTOs, service interfaces and application rules.
- `FinTrack.Infrastructure`: Entity Framework Core, PostgreSQL configuration and repository implementations.
- `FinTrack.Api`: HTTP controllers, Swagger and dependency injection composition.

The first MVP keeps the design simple. Controllers receive HTTP requests, call application services, and services use repository contracts. Infrastructure implements those contracts with EF Core.

Authentication is planned for a later phase. Until then, the API uses a fixed demo user id so the data model is already multi-user without adding JWT complexity too early.
