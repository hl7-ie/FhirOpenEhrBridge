# FHIR-OpenEHR-Bridge — Python

The Python port of the bidirectional FHIR ⇄ openEHR translation engine. Part of
the [polyglot monorepo](../README.md); behaviour matches the
[shared conformance contract](../docs/POLYGLOT.md).

The runtime has **no third-party dependencies** — the HTTP API uses the standard
library `http.server`.

## Install & test

```bash
cd python
python -m venv .venv && . .venv/bin/activate    # Windows: .venv\Scripts\activate
pip install -e ".[dev]"
pytest        # unit tests
behave        # BDD scenarios
```

## Run the API

```bash
python -m fhir_openehr_bridge.api    # listens on :8080 (PORT env to override)
```

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | raw FHIR `Patient` JSON | openEHR composition + issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + issues |
| `GET`  | `/health` | — | service health |

## CLI

```bash
fhir-openehr-bridge fhir-to-openehr ../samples/fhir/patient-full.json
fhir-openehr-bridge openehr-to-fhir ../samples/openehr/composition-demographics.json
```

## Library usage

```python
from fhir_openehr_bridge import TranslationService

svc = TranslationService()
result = svc.fhir_to_openehr(fhir_patient_json)
if result.success:
    print(result.value)  # openEHR composition dict
```

## Docker

```bash
docker build -t fhir-openehr-bridge-python .
docker run -p 8088:8080 fhir-openehr-bridge-python
```

Kubernetes manifests: [`deploy/k8s`](deploy/k8s).
