# Contributing to FHIR-OpenEHR-Bridge

Thanks for your interest in contributing! This document explains how to get set
up and the conventions the project follows.

## Code of conduct

Be respectful and constructive. Assume good intent.

## Getting started

```bash
git clone git@github.com:hl7-ie/FhirOpenEhrBridge.git
cd FhirOpenEhrBridge
dotnet restore
dotnet build
dotnet test
```

Requires the **.NET SDK 8.0+**. Docker is optional but recommended for running
the full local stack (see [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md)).

## Architecture rules (Clean Architecture)

Dependencies point inward only. Please keep it that way:

- `Domain` depends on **nothing**.
- `Application` depends on **Domain** only.
- `Infrastructure` depends on **Domain** + **Application**.
- `Api` depends on all three and is the composition root.

Ports (interfaces) live in `Application`/`Domain`; adapters (e.g. HTTP clients)
live in `Infrastructure`.

## Adding a new mapping

1. Implement `IFhirToOpenEhrMapper<T>` / `IOpenEhrToFhirMapper<T>` — or extend
   `FhirToOpenEhrMapperBase<T>` / `OpenEhrToFhirMapperBase<T>` to get the
   validate-then-map pipeline for free.
2. Register the mapper in `AddBridgeApplication` (both the marker and the
   generic interface).
3. Add **unit tests** for the mapper, and a **BDD scenario** if it changes
   observable behaviour.

## Testing

| Suite | Project | Framework |
| --- | --- | --- |
| Unit | `tests/FhirOpenEhrBridge.UnitTests` | xUnit + Moq |
| BDD | `tests/FhirOpenEhrBridge.BddTests` | Reqnroll (Gherkin) |
| Integration | `tests/FhirOpenEhrBridge.IntegrationTests` | xUnit + `WebApplicationFactory` |

Run everything with `dotnet test`. All suites must pass before a PR is merged;
CI enforces this.

## Coding conventions

- Standard Microsoft C# naming conventions.
- Nullable reference types are enabled — honour them.
- Document public APIs with XML doc comments.
- Keep methods small and intention-revealing; match the style of surrounding code.

## Commit & PR workflow

1. Branch from `main` (e.g. `feat/...`, `fix/...`, `docs/...`).
2. Make focused commits with clear messages (Conventional Commits encouraged).
3. Ensure `dotnet build` and `dotnet test` pass locally.
4. Open a PR against `main`. CI (build/test, CodeQL) must be green.
5. A maintainer reviews and merges.

## Licensing of contributions

By contributing you agree that your contributions are licensed under the
project's dual license: **MIT** for code and **CC BY 4.0** for documentation and
mapping models. See [`LICENSE-MIT.txt`](LICENSE-MIT.txt) and
[`LICENSE-CC-BY.txt`](LICENSE-CC-BY.txt).
