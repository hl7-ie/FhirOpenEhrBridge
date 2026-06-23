# FHIR-OpenEHR-Bridge — Java

The Java port of the bidirectional FHIR ⇄ openEHR translation engine. Part of
the [polyglot monorepo](../README.md); behaviour matches the
[shared conformance contract](../docs/POLYGLOT.md).

Uses Jackson for JSON and the JDK's bundled `com.sun.net.httpserver.HttpServer`
for the API (no Spring). Requires JDK 17+ and Maven.

## Build & test

```bash
cd java
mvn verify          # JUnit unit tests + Cucumber (Gherkin) BDD scenarios
```

## Run the API

```bash
mvn -q -DskipTests package
java -jar target/fhir-openehr-bridge.jar      # listens on :8080 (PORT env to override)
```

| Method | Route | Body | Returns |
| --- | --- | --- | --- |
| `POST` | `/api/translate/fhir-to-openehr` | raw FHIR `Patient` JSON | openEHR composition + issues |
| `POST` | `/api/translate/openehr-to-fhir` | openEHR composition JSON | FHIR `Bundle` + issues |
| `GET`  | `/health` | — | service health |

## CLI

```bash
mvn -q -DskipTests package
java -cp target/fhir-openehr-bridge.jar io.github.hl7ie.fhiropenehrbridge.Cli \
  fhir-to-openehr ../samples/fhir/patient-full.json
```

## Library usage

```java
import io.github.hl7ie.fhiropenehrbridge.TranslationService;

var service = new TranslationService();
var result = service.fhirToOpenEhr(fhirPatientJson);
if (result.success()) { /* result.value() is a Jackson JsonNode */ }
```

## Docker

```bash
docker build -t fhir-openehr-bridge-java .
docker run -p 8088:8080 fhir-openehr-bridge-java
```

Kubernetes manifests: [`deploy/k8s`](deploy/k8s).
