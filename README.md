# FHIR-OpenEHR-Bridge

A robust, fully tested, **bidirectional** mapping and translation engine between
HL7 **FHIR** (R4/R5) payloads and **openEHR** (RM/AQL) payloads.

[![Build and Test](https://github.com/hl7-ie/FhirOpenEhrBridge/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/hl7-ie/FhirOpenEhrBridge/actions/workflows/build-and-test.yml)
[![Code License: MIT](https://img.shields.io/badge/Code-MIT-blue.svg)](LICENSE-MIT.txt)
[![Docs License: CC BY 4.0](https://img.shields.io/badge/Docs-CC%20BY%204.0-lightgrey.svg)](LICENSE-CC-BY.txt)

---

## Purpose

Healthcare data lives in two dominant open standards that do not speak the same
language out of the box:

- **HL7 FHIR** — a resource-oriented REST API standard widely adopted for data
  exchange and app integration.
- **openEHR** — an archetype/template-driven Reference Model (RM) for long-term,
  vendor-neutral clinical data persistence, queried with AQL.

`FHIR-OpenEHR-Bridge` provides a clean, extensible translation layer so that
systems built on one standard can exchange data with the other without bespoke
point-to-point code. It ships with a working proof-of-concept that maps a FHIR
`Patient` to an openEHR demographics composition (and back to a FHIR `Bundle`).

## Architecture

The solution follows **Clean / Hexagonal Architecture**. Dependencies point
inward only:

```
        +-------------------+
        |        Api        |  ASP.NET Core Web API (composition root)
        +---------+---------+
                  |
   +--------------+--------------+
   |                             |
+--v---------+        +----------v-----+
| Infrastructure |    |  Application   |  mappers, validation, translation
| (adapters)     |--> |  (use cases /  |
+------+---------+    |   ports)       |
       |              +--------+-------+
       |                       |
       +-----------+-----------+
                   |
            +------v------+
            |   Domain    |  contracts + openEHR mapping models (no deps)
            +-------------+
```

| Project | Responsibility | Key dependencies |
| --- | --- | --- |
| `FhirOpenEhrBridge.Domain` | Mapping interfaces, validation primitives, openEHR mapping models. Pure, dependency-free. | — |
| `FhirOpenEhrBridge.Application` | The translation logic: mappers, validators, the `TranslationService`, and the outbound ports. | Firely SDK (`Hl7.Fhir.R4`) |
| `FhirOpenEhrBridge.Infrastructure` | Adapters: typed `HttpClient`s for external FHIR servers and openEHR CDRs. | `Microsoft.Extensions.Http` |
| `FhirOpenEhrBridge.Api` | REST endpoints, Swagger, DI composition root. | ASP.NET Core |

Architecture diagrams (C4 model + sequences) live in [`docs/architecture`](docs/architecture):

- [`c4-context.mmd`](docs/architecture/c4-context.mmd) — system context (Mermaid C4).
- [`c4-container.mmd`](docs/architecture/c4-container.mmd) — container view (Mermaid C4).
- [`c4-component.mmd`](docs/architecture/c4-component.mmd) — component view of the API (Mermaid C4).
- [`c4-deployment.mmd`](docs/architecture/c4-deployment.mmd) — Kubernetes deployment view (Mermaid C4).
- [`sequence-fhir-to-openehr.mmd`](docs/architecture/sequence-fhir-to-openehr.mmd) / [`sequence-openehr-to-fhir.mmd`](docs/architecture/sequence-openehr-to-fhir.mmd) — translation sequence diagrams.
- [`architecture-diagrams.drawio`](docs/architecture/architecture-diagrams.drawio) — editable Draw.io source.

## Tech stack

- **.NET 8** (C#) / **ASP.NET Core** Web API
- **Firely .NET SDK** (`Hl7.Fhir.R4`) for FHIR parsing/serialization
- System.Text.Json for openEHR payloads
- **xUnit** + **Moq** for unit tests
- **Reqnroll** (the maintained successor to SpecFlow) for BDD/Gherkin tests

> **Why Reqnroll instead of SpecFlow?** SpecFlow was discontinued by its owner in
> late 2024 and does not support modern .NET test tooling. Reqnroll is its
> community-maintained, drop-in fork — the `.feature` files and step bindings use
> the **identical Gherkin syntax**, so this is purely a tooling-currency choice.

## Getting started

### Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- (Optional) Docker, for the containerised API

> **Monorepo note:** the .NET reference implementation lives under
> [`dotnet/`](dotnet). Other language ports live in sibling folders
> (`nodejs/`, `go/`, `java/`, `rust/`, `python/`) — see
> [Languages & packages](#languages--packages). The shared cross-language
> conformance payloads live in [`samples/`](samples).

### Build & test (.NET)

```bash
cd dotnet
dotnet restore
dotnet build
dotnet test          # runs xUnit unit tests + Reqnroll BDD scenarios
```

### Run the API

```bash
dotnet run --project dotnet/src/FhirOpenEhrBridge.Api
```

Swagger UI is served at `https://localhost:<port>/swagger` in Development.

### Run with Docker

Single container:

```bash
docker build -t fhir-openehr-bridge -f dotnet/src/FhirOpenEhrBridge.Api/Dockerfile dotnet
docker run -p 8088:8080 fhir-openehr-bridge
```

Full local stack (API + a real HAPI FHIR server + EHRbase openEHR CDR + Postgres):

```bash
docker compose up --build
./scripts/smoke-test.sh        # verify all endpoints once it is up
```

See [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) for the Kubernetes (Kustomize) and
Argo CD (GitOps) deployment paths.

## API

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | Raw FHIR resource JSON (e.g. a `Patient`) | openEHR composition + validation issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + validation issues |
| `GET`  | `/health` | — | Service health |

### Example: FHIR → openEHR

```bash
curl -X POST http://localhost:8080/api/translate/fhir-to-openehr \
  -H "Content-Type: application/fhir+json" \
  -d '{
        "resourceType": "Patient",
        "id": "example-123",
        "name": [{ "family": "Smith", "given": ["John"] }],
        "gender": "male",
        "birthDate": "1980-05-15"
      }'
```

Response (abridged):

```json
{
  "success": true,
  "result": {
    "archetypeNodeId": "openEHR-EHR-COMPOSITION.demographics.v1",
    "ehrStatus": { "subjectId": "example-123", "subjectNamespace": "DEMOGRAPHIC" },
    "demographics": { "familyName": "Smith", "givenName": "John", "gender": "male", "birthDate": "1980-05-15" }
  },
  "issues": []
}
```

## Samples & demos

The [`samples/`](samples) folder demonstrates the engine three ways:

- **Console demo** — runs every sample payload through the engine in both
  directions (plus round-trip and rejection paths), no server required:
  ```bash
  dotnet run --project dotnet/samples/FhirOpenEhrBridge.Demo
  ```
- **`samples/requests.http`** — ready-to-run requests for the VS Code REST Client / Rider against the live API.
- **Sample payloads** — `samples/fhir/*.json` and `samples/openehr/*.json`, usable with `curl --data @<file>`.

There are also **Ireland & cross-border** examples — Individual Health Identifier
(IHI), all-island care (Republic of Ireland ↔ Northern Ireland), and EU
cross-border via MyHealth@EU / IPS. See
[`docs/IRELAND-CROSSBORDER.md`](docs/IRELAND-CROSSBORDER.md).

See [`samples/README.md`](samples/README.md) for details.

## How it works

Every translation runs a **validate-then-map** pipeline:

1. The `TranslationService` selects a mapper by the FHIR `resourceType`
   (or openEHR `archetypeNodeId`).
2. The mapper's base class validates the payload via an `IPayloadValidator`.
   Any `Error`-severity issue aborts the mapping.
3. On success the concrete mapper produces the output, carrying any non-fatal
   warnings onto the result.

Adding a new resource mapping is a matter of implementing
`IFhirToOpenEhrMapper<T>` / `IOpenEhrToFhirMapper<T>` (or extending the provided
base classes) and registering it in `AddBridgeApplication`.

## Testing strategy

- **Unit tests** (`dotnet/tests/FhirOpenEhrBridge.UnitTests`) — mapper logic, gender
  mapping, round-tripping, and `TranslationService` dispatch (using Moq).
- **BDD tests** (`dotnet/tests/FhirOpenEhrBridge.BddTests`) — Gherkin features describing
  the translation behaviour end-to-end:
  - `Features/FhirToOpenEhrTranslation.feature`
  - `Features/OpenEhrToFhirTranslation.feature`
- **Integration tests** (`dotnet/tests/FhirOpenEhrBridge.IntegrationTests`) — boot the
  real ASP.NET Core pipeline in-memory with `WebApplicationFactory<Program>` and
  exercise the HTTP endpoints.

## Languages & packages

This is a **polyglot monorepo**: the same FHIR ⇄ openEHR translation engine is
(being) implemented in several languages, each independently buildable,
testable, publishable, and deployable. The shared cross-language conformance
payloads live in [`samples/`](samples).

| Language | Folder | Status |
| --- | --- | --- |
| .NET (C#) | [`dotnet/`](dotnet) | ✅ Reference implementation |
| Node.js / TypeScript | [`nodejs/`](nodejs) | 🚧 In progress |
| Go | [`go/`](go) | 🚧 In progress |
| Java | [`java/`](java) | 🚧 In progress |
| Rust | [`rust/`](rust) | 🚧 In progress |
| Python | [`python/`](python) | 🚧 In progress |

Each port targets parity: core library + unit + BDD tests + HTTP API + container
image + per-language CI pipeline.

## CI/CD

GitHub Actions workflows in [`.github/workflows`](.github/workflows). Pipelines
are **per-language** and path-filtered, so a change under `go/` only runs the Go
pipeline, etc.

- **dotnet-build-and-test.yml** — restore, build, test, and publish a code-coverage report.
- **dotnet-codeql.yml** — CodeQL security/quality analysis for C#.
- **dotnet-publish-nuget.yml** — on a `v*.*.*` tag, pack `Domain` + `Application` and push to GitHub Packages (and NuGet.org if `NUGET_API_KEY` is set).
- **dotnet-docker-publish.yml** — build the API image and push it to GitHub Container Registry (`ghcr.io`).
- **cd-gitops.yml** — on a release tag, bump the production Kustomize overlay's image tag and commit it back to `main` for Argo CD to reconcile.
- Per-language `…-ci.yml` pipelines are added alongside each language port.

## Deployment

- **Local stack:** `docker compose up --build` (see [Run with Docker](#run-with-docker)).
- **Kubernetes:** Kustomize base + `dev`/`prod` overlays under [`deploy/k8s`](deploy/k8s).
- **GitOps:** Argo CD `AppProject` + per-environment `Application`s under [`deploy/argocd`](deploy/argocd).

Full instructions are in [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md).

## Contributing

Contributions are welcome! Please:

1. Fork the repository and create a feature branch.
2. Keep changes aligned with the Clean Architecture dependency rules (Domain
   depends on nothing; Application depends only on Domain; etc.).
3. Add/maintain tests — new mappers should ship with unit tests and, where they
   change observable behaviour, a BDD scenario.
4. Run `dotnet build` and `dotnet test` before opening a PR.
5. Follow standard Microsoft C# naming conventions and document public APIs.

## Licensing

This project is **dual-licensed**:

- **Source code** (`/src`, `/tests`, build & CI tooling) — [MIT License](LICENSE-MIT.txt).
- **Documentation & mapping models** (`/docs`, mapping definitions) — [Creative Commons Attribution 4.0 International (CC BY 4.0)](LICENSE-CC-BY.txt).

## Disclaimer

The mappings shipped here are a **proof of concept** for demonstration and
development. They are **not** clinically validated and must not be used in
production care settings without appropriate clinical safety review (e.g.
DCB0129 / DCB0160 in the UK) and conformance testing against your specific
FHIR profiles and openEHR templates.
