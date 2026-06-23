# FHIR-OpenEHR-Bridge — Node.js / TypeScript

The Node.js/TypeScript port of the bidirectional FHIR ⇄ openEHR translation
engine. Part of the [polyglot monorepo](../README.md); behaviour matches the
[shared conformance contract](../docs/POLYGLOT.md) and is validated against the
shared [`../samples`](../samples).

## Install & build

```bash
cd nodejs
npm install
npm run build
```

## Test

```bash
npm test          # Jest unit + API (supertest) tests
npm run test:bdd  # Cucumber (Gherkin) scenarios
```

## Run the API

```bash
npm start                       # node dist/main.js  (after build)
# or, without building:
npm run dev                     # ts-node src/main.ts
```

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | raw FHIR `Patient` JSON | openEHR composition + issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + issues |
| `GET`  | `/health` | — | service health |

## CLI

```bash
npm run cli -- fhir-to-openehr ../samples/fhir/patient-full.json
npm run cli -- openehr-to-fhir ../samples/openehr/composition-demographics.json
```

## Library usage

```ts
import { TranslationService } from '@hl7-ie/fhir-openehr-bridge';

const service = new TranslationService();
const result = service.fhirToOpenEhr(fhirPatientJson);
if (result.success) console.log(result.value); // OpenEhrComposition
```

## Docker

```bash
docker build -t fhir-openehr-bridge-nodejs .
docker run -p 8088:8080 fhir-openehr-bridge-nodejs
```

Kubernetes manifests: [`deploy/k8s`](deploy/k8s).
