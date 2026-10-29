---
name: 'C# testing'
description: 'Testing guidelines for C# test projects'
applyTo: 'tests/**/*.cs'
---

# Testing Guidelines

## Framework & Naming
- Use **xUnit** for all tests. 
- Test class names mirror the class under test: `OrderService` → `OrderServiceTests`.
- Name test methods: `MethodName_Scenario_ExpectedResult`
  (e.g. `Cancel_WhenOrderIsShipped_ReturnsFailure`).
- One behavior per test method. 

## Structure
- Follow **Arrange – Act – Assert** with clear section separation (blank line or comment).
- No branching (`if`, `for`, `switch`) inside test bodies.
- No disk I/O; if unavoidable, randomize paths and log file locations.
- Tests must be order-independent and parallel-safe. 

## Test doubles
- Prefer hand-written **fakes** or in-memory doubles over external mock libraries when testing repository, unit-of-work and service behavior.
- Never mock code that is part of the solution under test.
- Use test doubles only at external boundaries or persistence seams (repositories, unit-of-work factories, HTTP clients, messaging).
- Prefer testing observable behavior (return values, exceptions, state changes) over verifying internal calls.
- If a dependency is complex to fake, keep the test double small and explicit rather than introducing a broad mocking framework.

## Integration Tests
- Use `WebApplicationFactory<TProgram>` for API integration tests.
- Use a real in-memory database (SQLite in-memory or Testcontainers with PostgreSQL).
- Do NOT mock the database in integration tests. 

## Assertions
- Prefer **FluentAssertions** (`Should()`) over raw xUnit asserts for readability.
- One primary assertion per test; if you need more, split into separate tests.
- Assert specific values and edge cases, not vague outcomes. 

## Parameterized Tests
- Use `[Theory]` + `[InlineData]` when testing multiple inputs for the same behavior.
- Use `[Theory]` + `[MemberData]` for complex or generated test data. 

## General
- Avoid magic strings — assign hardcoded values to constants.
- Test through **public APIs** only; do not change visibility or use `InternalsVisibleTo`. 
- Require tests for every new or changed public API.
- Avoid comments like `// Arrange`, `// Act`, `// Assert` in test methods; rely on clear section separation instead.