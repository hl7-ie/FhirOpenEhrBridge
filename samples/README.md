# Samples & Demos

This folder demonstrates the FHIR-OpenEHR-Bridge translation engine three ways.

## Layout

```
samples/
  fhir/                       # example FHIR resources (inputs for fhir-to-openehr)
    patient-full.json         #   id, two identifiers, name, gender, dob, address
    patient-minimal.json      #   just name + gender
    observation-invalid.json  #   wrong resource type -> rejected
  openehr/                    # example openEHR compositions (inputs for openehr-to-fhir)
    composition-demographics.json  # full demographics
    composition-minimal.json       # name + gender only
    composition-invalid.json       # null demographics -> rejected
  FhirOpenEhrBridge.Demo/     # runnable console demo (uses the Application library directly)
  requests.http               # ready-to-run HTTP requests against the live API
```

## 1. Console demo (no server needed)

Composes the Application layer in-process and runs every sample through the
engine in both directions, plus a round-trip and the rejection paths:

```bash
dotnet run --project samples/FhirOpenEhrBridge.Demo
```

Expected highlights:

- `patient-full.json` / `patient-minimal.json` → openEHR demographics compositions
- `observation-invalid.json` → **rejected** (no mapper for `Observation`)
- `composition-demographics.json` / `composition-minimal.json` → FHIR `Bundle`s
- `composition-invalid.json` → **rejected** (missing demographics payload)
- Round trip: `patient-full-001` survives FHIR → openEHR → FHIR with id, name,
  gender and identifier intact

## 2. Live API (`requests.http`)

Start the API, then fire the requests from your editor's HTTP client:

```bash
dotnet run --project src/FhirOpenEhrBridge.Api
# default URL is http://localhost:5199 (matches @host in requests.http)
```

Open [`requests.http`](requests.http) in VS Code (REST Client extension) or Rider
and click **Send Request** on each block.

## 3. Full stack + curl

Bring up the API with real backends and run the scripted smoke test:

```bash
docker compose up --build
./scripts/smoke-test.sh          # uses http://localhost:8088
```

You can also `curl` any sample file directly:

```bash
curl -X POST http://localhost:8088/api/translate/fhir-to-openehr \
  -H "Content-Type: application/fhir+json" \
  --data @samples/fhir/patient-full.json
```
