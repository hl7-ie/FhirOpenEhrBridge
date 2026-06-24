# Deployment Guide

This guide covers running the FHIR-OpenEHR-Bridge locally with Docker and
deploying it to Kubernetes via Argo CD (GitOps).

## 1. Local stack (Docker Compose)

The [`docker-compose.yml`](../docker-compose.yml) at the repo root brings up the
API together with **real** backends so you can exercise the integrations:

| Service | URL | Notes |
| --- | --- | --- |
| `bridge-api` | http://localhost:8088 | The API under test (Swagger at `/swagger`) |
| `hapi-fhir` | http://localhost:8082/fhir | HL7 FHIR R4 server |
| `ehrbase` | http://localhost:8080/ehrbase | openEHR CDR |
| `ehrdb` | localhost:5432 | Postgres backing EHRbase |

```bash
docker compose up --build
```

The API is pre-wired (via environment variables) to reach `hapi-fhir` and
`ehrbase` over the compose network. EHRbase can take a minute to become healthy
on first start while it runs its database migrations.

Smoke-test once it is up:

```bash
./scripts/smoke-test.sh            # or: bash scripts/smoke-test.sh
```

Tear down (and wipe the database volume):

```bash
docker compose down -v
```

## 2. Container image

The API image is built from
[`dotnet/src/FhirOpenEhrBridge.Api/Dockerfile`](../dotnet/src/FhirOpenEhrBridge.Api/Dockerfile)
(multi-stage, runs as non-root on port 8080) and published to GitHub Container
Registry by the `docker-publish` workflow:

```
ghcr.io/hl7-ie/fhiropenehrbridge:<tag>
```

Build locally:

```bash
docker build -t fhir-openehr-bridge:local -f dotnet/src/FhirOpenEhrBridge.Api/Dockerfile dotnet
docker run -p 8088:8080 fhir-openehr-bridge:local
```

## 3. Kubernetes (Kustomize)

Manifests live under [`deploy/k8s`](../deploy/k8s) using a base + overlays layout:

```
deploy/k8s/
  base/            # Deployment, Service, ConfigMap, Secret, HPA, Ingress, Namespace
  overlays/dev/    # 1 replica, image tag :dev, namespace fhir-openehr-bridge-dev
  overlays/prod/   # 3 replicas, image tag :latest, TLS ingress
```

Render and apply directly (without Argo CD):

```bash
kubectl kustomize deploy/k8s/overlays/dev        # preview
kubectl apply -k deploy/k8s/overlays/dev         # apply
```

The container is hardened: non-root, read-only root filesystem (with a writable
`/tmp` emptyDir), dropped capabilities, liveness/readiness probes on `/health`.

> **Secrets:** `base/secret.yaml` is a placeholder. In real clusters manage the
> CDR credentials with Sealed Secrets / External Secrets Operator / your cloud
> secret manager — do not commit real values.

## 4. GitOps with Argo CD

[`deploy/argocd`](../deploy/argocd) contains an `AppProject` and one
`Application` per environment, each tracking an overlay on `main`:

```bash
kubectl apply -f deploy/argocd/project.yaml
kubectl apply -f deploy/argocd/application-dev.yaml
kubectl apply -f deploy/argocd/application-prod.yaml
```

- **dev** — automated sync with prune + self-heal.
- **prod** — automated sync with self-heal but **prune disabled** for safety.

### Release flow (pull-based)

1. Tag a release: `git tag v1.2.3 && git push --tags`.
2. `docker-publish` builds & pushes `ghcr.io/hl7-ie/fhiropenehrbridge:1.2.3`.
3. `cd-gitops` updates the prod overlay's image tag on `main`.
4. Argo CD detects the change and reconciles the new image into the cluster.
