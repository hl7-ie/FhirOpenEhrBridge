# FHIR-OpenEHR-Bridge — Go

The Go port of the bidirectional FHIR ⇄ openEHR translation engine. Part of the
[polyglot monorepo](../README.md); behaviour matches the
[shared conformance contract](../docs/POLYGLOT.md).

Module: `github.com/hl7-ie/FhirOpenEhrBridge/go`

## Build & test

```bash
cd go
go build ./...
go vet ./...
go test ./...          # bridge unit tests + godog BDD features
```

## Run the API

```bash
go run ./cmd/api        # listens on :8080 (PORT env to override)
```

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | raw FHIR `Patient` JSON | openEHR composition + issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + issues |
| `GET`  | `/health` | — | service health |

## CLI

```bash
go run ./cmd/cli fhir-to-openehr ../samples/fhir/patient-full.json
go run ./cmd/cli openehr-to-fhir ../samples/openehr/composition-demographics.json
```

## Library usage

```go
import "github.com/hl7-ie/FhirOpenEhrBridge/go/bridge"

svc := bridge.NewTranslationService()
res := svc.FhirToOpenEhr(fhirPatientJSON)
if res.Success { /* res.Value is *bridge.OpenEhrComposition */ }
```

## Docker

```bash
docker build -t fhir-openehr-bridge-go .
docker run -p 8088:8080 fhir-openehr-bridge-go
```

Kubernetes manifests: [`deploy/k8s`](deploy/k8s).
