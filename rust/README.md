# FHIR-OpenEHR-Bridge — Rust

The Rust port of the bidirectional FHIR ⇄ openEHR translation engine. Part of
the [polyglot monorepo](../README.md); behaviour matches the
[shared conformance contract](../docs/POLYGLOT.md).

## Build & test

```bash
cd rust
cargo build
cargo test          # lib unit tests + cucumber BDD (tests/bdd.rs)
```

## Run the API

```bash
cargo run --bin api     # listens on :8080 (PORT env to override)
```

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | raw FHIR `Patient` JSON | openEHR composition + issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + issues |
| `GET`  | `/health` | — | service health |

## CLI

```bash
cargo run --bin cli -- fhir-to-openehr ../samples/fhir/patient-full.json
cargo run --bin cli -- openehr-to-fhir ../samples/openehr/composition-demographics.json
```

## Library usage

```rust
use fhir_openehr_bridge::TranslationService;

let svc = TranslationService::new();
let outcome = svc.fhir_to_openehr(fhir_patient_json);
if outcome.success { /* outcome.value: Option<OpenEhrComposition> */ }
```

## Docker

```bash
docker build -t fhir-openehr-bridge-rust .
docker run -p 8088:8080 fhir-openehr-bridge-rust
```

Kubernetes manifests: [`deploy/k8s`](deploy/k8s).

> **Local Windows note:** the `windows-gnu` Rust toolchain needs `dlltool`. If
> you hit `error calling dlltool 'dlltool.exe'`, put LLVM's `llvm-dlltool` on
> PATH as `dlltool` (or use the MSVC toolchain). CI (Linux) is unaffected.
