# Contributing to NFSe Nacional SDK for .NET

Thank you for your interest in contributing.

This project aims to be a clean, reusable, and ERP-agnostic .NET SDK for Brazil's NFSe National Standard.  
To keep the codebase maintainable and consistent, please follow the guidelines below.

## General Principles

- Keep the SDK independent from specific ERP systems or proprietary business rules
- Prefer small, focused, and easy-to-review pull requests
- Preserve separation of concerns between contracts, core, serialization, transport, and orchestration
- Avoid premature abstraction
- Prioritize readability, explicitness, and maintainability

## Development Setup

1. Clone the repository
2. Restore dependencies
3. Build the solution
4. Run tests

```bash
dotnet restore
dotnet build
dotnet test
```

## Branch Naming

Use descriptive branch names.

Examples:

- `feat/add-initial-client-contract`
- `fix/xml-serialization-null-handling`
- `docs/add-readme-and-contributing`
- `refactor/simplify-http-transport`

## Commit Convention

Use Conventional Commits whenever possible.

### Recommended types

- `feat`: a new feature
- `fix`: a bug fix
- `docs`: documentation changes
- `refactor`: code change that neither fixes a bug nor adds a feature
- `test`: adding or updating tests
- `build`: build system or dependency changes
- `ci`: CI/CD related changes
- `chore`: maintenance tasks

### Examples

- `feat: add initial DPS request contract`
- `fix: normalize API transport error handling`
- `docs: add pull request guidelines`
- `refactor: extract XML serializer abstraction`
- `test: add unit tests for endpoint configuration`

## Pull Request Guidelines

Before opening a pull request:

- Make sure the solution builds successfully
- Make sure tests pass
- Keep the scope focused
- Update documentation when relevant
- Explain the reason for the change

A pull request should include:

- What changed
- Why it changed
- Any architectural impact
- Notes about testing performed

## Coding Guidelines

- Use modern C# and .NET practices
- Keep nullable reference types enabled
- Prefer explicit names over unclear abbreviations
- Keep public APIs stable and intentional
- Avoid leaking infrastructure concerns into domain contracts
- Avoid introducing ERP-specific assumptions into the SDK

## Project Boundaries

This SDK should contain:

- Generic NFSe Nacional contracts
- Generic integration workflows
- Serialization logic
- Transport abstractions and implementations
- Error normalization
- Reusable integration components

This SDK should **not** contain:

- ERP-specific rules
- Company-specific business flows
- Database persistence concerns
- UI logic
- Proprietary integrations unrelated to NFSe Nacional

## Tests

Contributions that affect behavior should include tests whenever possible.

Preferred test areas:

- Request and response mapping
- Serialization and deserialization
- Endpoint and environment configuration
- Client behavior
- Error handling

## Discussions

For larger changes, open an issue or start a discussion before implementing significant architectural changes.

## Code of Conduct

Be respectful, objective, and constructive in discussions and reviews.
