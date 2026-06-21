# ADR-001: Architecture Baseline and Safety Net

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-06-21 |
| **Context** | Pre-migration safety net before DDD modular monolith refactoring |

## Context

BanquetHallManagement is an ABP 10.3 layered application. Before extracting bounded contexts into independent DDD modules, we need an automated baseline that detects layer violations without changing production behavior.

This ADR records the current dependency graph, the architecture rules under test, and the warning-only enforcement policy.

## Current Dependency Graph

### Project reference chain (compile-time)

```
BanquetHallManagement.Domain.Shared
    ↑
    ├── BanquetHallManagement.Domain
    │     ↑
    │     ├── BanquetHallManagement.Application ──→ BanquetHallManagement.Application.Contracts
    │     └── BanquetHallManagement.EntityFrameworkCore
    │
    └── BanquetHallManagement.Application.Contracts
          ↑
          ├── BanquetHallManagement.HttpApi
          ├── BanquetHallManagement.HttpApi.Client
          └── BanquetHallManagement.DbMigrator ──→ EntityFrameworkCore

BanquetHallManagement.HttpApi.Host
    → Application, HttpApi, EntityFrameworkCore
```

### Layer responsibilities

| Layer | Project | May reference |
|-------|---------|---------------|
| Shared Kernel | `Domain.Shared` | Nothing (project refs) |
| Domain | `Domain` | `Domain.Shared`, ABP domain packages |
| Application Contracts | `Application.Contracts` | `Domain.Shared` |
| Application | `Application` | `Domain`, `Application.Contracts`, ABP application packages |
| Infrastructure | `EntityFrameworkCore` | `Domain`, ABP EF packages |
| Presentation | `HttpApi` | `Application.Contracts` |
| Host | `HttpApi.Host` | `Application`, `HttpApi`, `EntityFrameworkCore` |

### Test project chain

```
BanquetHallManagement.TestBase
    → BanquetHallManagement.Domain.Tests
        → BanquetHallManagement.Application.Tests
            → BanquetHallManagement.EntityFrameworkCore.Tests

BanquetHallManagement.Architecture.Tests (standalone baseline)
    → Domain, Application (for assembly inspection only)
```

## Architecture Rules (Baseline)

Enforced by `BanquetHallManagement.Architecture.Tests` using **NetArchTest.Rules**:

| Rule | Scope | Forbidden dependencies |
|------|-------|------------------------|
| R1 | Domain assembly | `Microsoft.EntityFrameworkCore`, `BanquetHallManagement.EntityFrameworkCore`, `Volo.Abp.EntityFrameworkCore` |
| R2 | Domain assembly | `BanquetHallManagement.Application`, `Volo.Abp.Application` |
| R3 | Application assembly | `Microsoft.EntityFrameworkCore`, `BanquetHallManagement.EntityFrameworkCore`, `Volo.Abp.EntityFrameworkCore` |

A complementary **project reference validation** test inspects `Assembly.GetReferencedAssemblies()` for the Domain and Application assemblies to catch compile-time boundary leaks early.

## Enforcement Mode: Warning Only

`ArchitectureTestMode.EnforceStrictRules` is set to **`false`**.

- Violations are written to test output as `[ARCHITECTURE][WARNING]`.
- Tests always pass so CI is not blocked during the migration preparation phase.
- When the baseline is clean, flip `EnforceStrictRules` to `true` to make violations fail the build.

## Known Baseline Observations

These are documented for future remediation; they are **not** fixed in this ADR:

1. `BanquetHallManagementDbMigrationService` (Domain) contains string-based references to the EF Core project folder path — infrastructure concern in Domain.
2. Several domain services use `GetQueryableAsync()` — persistence query surface in Domain (target for Phase 2 migration).
3. `Application.Contracts` references `Microsoft.EntityFrameworkCore.Design` — contracts layer pollution.
4. Finance domain services reference `Reservation` entities directly — cross-bounded-context coupling.

## Decision

1. Add `test/BanquetHallManagement.Architecture.Tests` with NetArchTest.Rules.
2. Run architecture tests in warning mode alongside existing unit and integration tests.
3. Do not refactor production code as part of this safety net.
4. Enable strict mode only after recorded violations reach zero.

## Consequences

### Positive

- Layer violations become visible in every test run.
- Provides a measurable gate for DDD module extraction.
- Zero impact on runtime behavior.

### Negative

- NetArchTest.Rules (v1.3.2) is unmaintained; consider migrating to `NetArchTest.eNhancedEdition` in a future ADR if needed.
- Warning mode requires discipline — violations can be ignored until strict mode is enabled.

## Validation

```bash
dotnet test test/BanquetHallManagement.Architecture.Tests/BanquetHallManagement.Architecture.Tests.csproj
```

Review test output for `[ARCHITECTURE][WARNING]` entries. All tests should pass in warning mode.

## References

- [ABP Layered Solution Template](https://abp.io/docs/latest/solution-templates/layered-web-application)
- [NetArchTest.Rules](https://www.nuget.org/packages/NetArchTest.Rules/)
- Migration roadmap: DDD modular monolith plan (conversation baseline)
