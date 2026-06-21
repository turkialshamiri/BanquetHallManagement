# Module extraction scaffold

This folder will host independent ABP layered modules during the DDD modular monolith migration.

No code has been moved yet. Each subfolder is a placeholder for a future bounded-context module:

- `catalog/` — Halls, Customers, Services
- `reservations/` — Reservation lifecycle and scheduling
- `finance/` — Payments, invoicing, journal entries, refunds
- `reporting/` — Cross-context read models and reports

See `docs/adr/002-bounded-contexts.md` for the ownership map.
