---
id: SPEC-NNN
title:
status: draft
created: YYYY-MM-DD
updated: YYYY-MM-DD
author:
branch:
related: []
---

# Spec: <feature-title>

## Motivation
> Why are we building this? What problem does it solve?

## In Scope
-

## Out of Scope
-

## Domain Changes
<!-- If no domain changes, write "None" and skip sub-sections -->

### New Aggregates / Entities
-

### Modified Aggregates / Entities
-

### New Value Objects
-

### Domain Events
-

### Domain Services
<!-- Logic that doesn't belong to a single aggregate -->
-

### Business Rules
-

## Application Layer
<!-- Use cases to create or modify -->

### Use Cases
-

### Port Interfaces (Abstractions)
<!-- New IRepository / IService interfaces needed -->
-

## Infrastructure Layer

### Repositories
-

### EF Configurations
-

### Migrations
-

### External Services
-

## Contracts Layer
<!-- Naming: XxxRequest / XxxResponse for top-level endpoint contracts, XxxDto for nested / reusable shapes (e.g. items in a list) -->

### Requests
-

### Responses
-

### Dtos
<!-- Nested shapes reused across responses, e.g. ItemDto inside GetItemsResponse -->
-

## Api Layer

### Endpoints
<!-- Method, route, request/response shape -->
-

### Validators
-

## Web Layer

### Pages / Components
<!-- Route, purpose, interactive or static -->
-

### API Calls
<!-- Which endpoints this page/component hits -->
-

## Dependencies
- Other specs:
- External services:
- Must be done first:

## Acceptance Criteria
- [ ]

## Implementation Checklist
- [ ] Domain: entities / value objects
- [ ] Domain: domain events
- [ ] Domain: domain services
- [ ] Domain: business rules
- [ ] Application: use case(s)
- [ ] Application: port interfaces
- [ ] Infrastructure: repository implementation(s)
- [ ] Infrastructure: EF configuration(s)
- [ ] Infrastructure: migration
- [ ] Contracts: request / response / dto records
- [ ] Web: page(s) / component(s)
- [ ] Api: endpoint(s)
- [ ] Api: validator(s)
- [ ] Unit tests: domain
- [ ] Unit tests: application
- [ ] Integration tests

## Open Questions
-

## Technical Notes
<!-- Architecture decisions worth capturing, e.g. "using outbox pattern via MassTransit" -->
-