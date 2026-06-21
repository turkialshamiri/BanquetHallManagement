# ADR-002: Bounded Contexts and Module Map

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-06-21 |
| **Context** | Phase 1 — formalize bounded contexts before physical module extraction |
| **Depends on** | [ADR-001: Architecture Baseline](./001-architecture-baseline.md) |

## Context

BanquetHallManagement is a layered ABP monolith with business capabilities organized as folders inside shared projects. Before extracting code into `modules/`, we formalize bounded contexts as real ABP domain modules and introduce read-only cross-module contracts.

This ADR records ownership, current code locations, module dependencies, and the target extraction layout. **No business code is moved in this phase.**

## Bounded Context Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     BanquetHallManagement (Monolith)                     │
├──────────────┬──────────────┬──────────────┬──────────────┬─────────────┤
│  Catalog     │ Reservations │   Finance    │  Reporting   │ Supporting  │
│  (master     │ (booking     │ (payments,   │ (read models │             │
│   data)      │  lifecycle)  │  accounting) │  & exports)  │             │
├──────────────┼──────────────┼──────────────┼──────────────┼─────────────┤
│ Hall         │ Reservation  │ Payment      │ ReportsApp   │ Dashboard   │
│ Customer     │ Reservation  │ Invoice      │ IReportQuery │ Identity    │
│ Service      │   Service    │ JournalEntry │   Executor   │ Tenant      │
│              │ Scheduling   │ Account      │              │ Settings    │
│              │ Payment Mon. │ Refund       │              │             │
│              │              │ AccessCard   │              │             │
└──────────────┴──────────────┴──────────────┴──────────────┴─────────────┘
```

## Context Ownership

### Catalog

| Item | Current location |
|------|------------------|
| **Domain module** | `Domain/Catalog/CatalogModule.cs` → `CatalogDomainModule` |
| **Entities** | `Domain/Halls/`, `Domain/Customers/`, `Domain/Services/` |
| **App services** | `Application/` (Hall, Customer, Service app services) |
| **Contracts** | `Application.Contracts/Halls/`, `Customers/`, `Services/` |
| **Lookup contract** | `Application.Contracts/Catalog/ICatalogLookupAppService` |
| **Angular** | `angular/src/app/features/halls/`, `customers/`, `services/` |

**Owns:** `Hall`, `Customer`, `Service` aggregates and master-data invariants.

**Depends on:** Shared Kernel only (no other bounded contexts).

---

### Reservations

| Item | Current location |
|------|------------------|
| **Domain module** | `Domain/Reservations/ReservationsModule.cs` → `ReservationsDomainModule` |
| **Entities** | `Domain/Reservations/`, `Domain/ReservationServices/` |
| **Domain services** | `ReservationSchedulingManager`, `ReservationPaymentMonitorService`, `HallAvailabilityManager` |
| **App services** | `Application/Reservations/` |
| **Contracts** | `Application.Contracts/Reservations/` |
| **Lookup contract** | `Application.Contracts/Reservations/IReservationLookupAppService` |
| **Events** | `Domain/Reservations/Events/`, `Application.Contracts/Events/Reservations/` |
| **Angular** | `angular/src/app/features/bookings/` |

**Owns:** `Reservation` aggregate, scheduling rules, reservation lifecycle, hall entry confirmation.

**Depends on:** Catalog (read-only via `ICatalogLookupAppService` after implementation).

**Known coupling (to remediate later):** Finance domain services directly reference `Reservation` entities.

---

### Finance

| Item | Current location |
|------|------------------|
| **Domain module** | `Domain/Finance/FinanceModule.cs` → `FinanceDomainModule` |
| **Entities** | `Domain/Finance/` (Payments, Invoices, JournalEntries, Accounts, HallAccessCards, Sequences) |
| **Domain services** | `PaymentManager`, `InvoiceManager`, `JournalPostingService`, `RevenueRecognitionService`, `RefundLiabilityService`, etc. |
| **App services** | `Application/Finance/` |
| **Contracts** | `Application.Contracts/Finance/` |
| **Lookup contract** | `Application.Contracts/Finance/IFinanceLookupAppService` |
| **Event handlers** | `Application/EventHandlers/Finance/` |
| **Angular** | `angular/src/app/features/finance/` |

**Owns:** Payment recording, invoicing, journal posting, revenue recognition, refunds, access cards.

**Depends on:** Reservations (read-only via `IReservationLookupAppService` after implementation; today: direct entity access).

**Reacts to:** Reservation domain events (`PaymentReceived`, `FullyPaid`, etc.).

---

### Reporting

| Item | Current location |
|------|------------------|
| **Domain** | `Domain/Reports/IReportQueryExecutor.cs` |
| **App services** | `Application/Reports/` |
| **Contracts** | `Application.Contracts/Reports/` |
| **Infrastructure** | `EntityFrameworkCore/Reports/EfCoreReportQueryExecutor.cs` |
| **Angular** | `angular/src/app/features/reports/` |

**Owns:** Cross-context report queries and export DTOs.

**Depends on:** All contexts via Application.Contracts (read-only).

**Note:** Not yet formalized as an `AbpModule`; will be added when extracted to `modules/reporting/`.

---

### Supporting (not extracted in Phase 1)

| Capability | Location |
|------------|----------|
| Dashboard | `Domain/Dashboard/`, `Application/Dashboard/` |
| Identity / Tenant / Settings | ABP platform modules + seed contributors |

## ABP Domain Module Registration

`BanquetHallManagementDomainModule` now depends on:

```csharp
[DependsOn(
    typeof(BanquetHallManagementDomainSharedModule),
    typeof(CatalogDomainModule),
    typeof(ReservationsDomainModule),
    typeof(FinanceDomainModule),
    // ... ABP platform domain modules
)]
```

Each bounded-context domain module depends only on `BanquetHallManagementDomainSharedModule` — **no cross-context domain module dependencies**.

## Cross-Module Contracts (read-only)

Introduced in `Application.Contracts` without implementations:

| Interface | Purpose | Methods |
|-----------|---------|---------|
| `IReservationLookupAppService` | Finance / Reporting read reservation data | `GetAsync`, `ExistsAsync` |
| `IFinanceLookupAppService` | Reservations / Reporting read finance data | `GetAsync`, `ExistsAsync` |
| `ICatalogLookupAppService` | Reservations read master data | `GetHallAsync`, `HallExistsAsync`, `GetCustomerAsync`, `CustomerExistsAsync`, `GetServiceAsync`, `ServiceExistsAsync` |

Implementations will be added when modules are extracted. Until then, existing app services and direct references continue to work unchanged.

## Target Physical Layout (`modules/`)

Scaffold created; extraction not started:

```
modules/
├── catalog/        → future BanquetHallManagement.Catalog.*
├── reservations/   → future BanquetHallManagement.Reservations.*
├── finance/        → future BanquetHallManagement.Finance.*
└── reporting/      → future BanquetHallManagement.Reporting.*
```

Each extracted module will follow the standard ABP 5-layer pattern:

```
Domain.Shared → Domain → Application.Contracts → Application → EntityFrameworkCore
```

## Inter-Context Communication Rules

| From → To | Allowed | Mechanism |
|-----------|---------|-----------|
| Finance → Reservations | Yes | `IReservationLookupAppService`, domain events |
| Reservations → Finance | No direct domain refs | Finance reacts via events |
| Reservations → Catalog | Yes | `ICatalogLookupAppService` |
| Reporting → Any | Yes | Contracts + query interfaces |
| Domain → Domain (cross-context) | **No** | Use contracts or events |

## Extraction Order (planned)

1. **Catalog** — zero outbound coupling
2. **Reservations** — depends on Catalog contracts
3. **Finance** — depends on Reservations contracts + events
4. **Reporting** — depends on all via contracts

## Decision

1. Convert `FinanceDomainModule` from static marker to `AbpModule`.
2. Add `ReservationsDomainModule` and `CatalogDomainModule` as `AbpModule`.
3. Register all three in `BanquetHallManagementDomainModule`.
4. Add read-only lookup contracts (interfaces + minimal DTOs only).
5. Create `modules/` scaffold without moving code.

## Consequences

### Positive

- Bounded contexts are visible in ABP module graph.
- Cross-module contracts are defined before extraction.
- Compile-time and runtime behavior unchanged.

### Negative

- Lookup interfaces are unused until implemented (intentional).
- Folder-based code still coexists with formal modules until Phase 5 extraction.

## Validation

```bash
dotnet build src/BanquetHallManagement.HttpApi.Host/BanquetHallManagement.HttpApi.Host.csproj
dotnet test test/BanquetHallManagement.Architecture.Tests/BanquetHallManagement.Architecture.Tests.csproj
```

Application must start; existing APIs and tests must pass.

## References

- [ADR-001: Architecture Baseline](./001-architecture-baseline.md)
- [ABP Modularity](https://abp.io/docs/latest/framework/architecture/modularity/basics)
