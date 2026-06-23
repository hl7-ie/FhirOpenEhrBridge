# Polyglot monorepo plan

The FHIR ⇄ openEHR translation engine is implemented in multiple languages so
teams can adopt the bridge in whatever stack they already run. Every language
port is an independent, publishable component with its own build, tests, and
CI pipeline, but all of them implement the **same behaviour** and are validated
against the **same shared conformance payloads** in [`../samples`](../samples).

## Repository layout

```
FhirOpenEhrBridge/
├── dotnet/     # .NET 8 / C#      (reference implementation)
├── nodejs/     # Node.js / TypeScript
├── go/         # Go
├── java/       # Java
├── rust/       # Rust
├── python/     # Python
├── samples/    # SHARED conformance payloads (fhir/*.json, openehr/*.json)
├── deploy/     # SHARED Kubernetes (Kustomize) + Argo CD manifests
└── docs/       # SHARED documentation & architecture diagrams
```

## Parity target per language

Each port aims for the same surface area as the .NET reference:

| Capability | Notes |
| --- | --- |
| Core translation library | `Patient` ⇄ openEHR demographics composition, both directions |
| Validation | validate-then-map pipeline; reject unsupported resource types / malformed input |
| Multiple identifiers | preserve all national identifiers (e.g. IHI + NHS) — see [Ireland examples](IRELAND-CROSSBORDER.md) |
| Unit tests | mapper logic, gender mapping, round-trip |
| BDD tests | Gherkin features mirroring `FhirToOpenEhrTranslation` / `OpenEhrToFhirTranslation` |
| HTTP API | `POST /api/translate/fhir-to-openehr`, `POST /api/translate/openehr-to-fhir`, `GET /health` |
| Container image | published to `ghcr.io` |
| CI pipeline | path-filtered `…-ci.yml` (build, test, lint, publish) |

## Shared conformance contract

The JSON payloads in [`../samples`](../samples) are the cross-language contract.
Every implementation must:

- **FHIR → openEHR:** accept each `samples/fhir/*.json` and produce a demographics
  composition with the expected `familyName`, `givenName`, `gender`, `birthDate`,
  identifiers and address; reject `observation-invalid.json`.
- **openEHR → FHIR:** accept each `samples/openehr/*.json` and produce a FHIR
  `Bundle` containing a `Patient` with the expected fields; reject
  `composition-invalid.json` (missing demographics).

A round trip (`FHIR → openEHR → FHIR`) must preserve id, name, gender and the
first identifier. Each language's BDD suite encodes these expectations, so the
shared samples keep all ports behaviourally aligned.

## Canonical model (language-neutral)

To stay dependency-light, ports map the **known subset** of FHIR `Patient` used
by the demographics scenario rather than embedding a full FHIR SDK (the .NET
reference uses Firely; other ports parse the subset directly):

**openEHR demographics composition**
- `archetypeNodeId`, `name`, `language`, `territory`, `category`, `startTime`
- `ehrStatus`: `subjectId`, `subjectNamespace`, `isQueryable`, `isModifiable`
- `demographics`: `familyName`, `givenName`, `gender`, `birthDate`,
  `identifiers[] { id, issuer, type }`, `address { line, city, postalCode, country }`

**Gender mapping** (FHIR ↔ openEHR): `male ↔ male`, `female ↔ female`,
`other ↔ intersex`, else `unknown`.

## Status

All six ports are implemented with full parity (library + unit + BDD tests +
HTTP API + Dockerfile + K8s manifests + path-filtered CI):

| Language | Folder | API | Unit | BDD |
| --- | --- | --- | --- | --- |
| .NET (C#) | `dotnet/` | ASP.NET Core | xUnit | Reqnroll |
| Node.js / TypeScript | `nodejs/` | Express | Jest | Cucumber |
| Go | `go/` | net/http | `go test` | godog |
| Java | `java/` | JDK HttpServer | JUnit 5 | Cucumber-JVM |
| Rust | `rust/` | axum | `cargo test` | cucumber-rs |
| Python | `python/` | stdlib http.server | pytest | behave |

See the **Languages & packages** table in the [root README](../README.md).
