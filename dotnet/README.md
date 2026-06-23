# FHIR-OpenEHR-Bridge — .NET implementation

The reference implementation of the bidirectional FHIR ⇄ openEHR translation
engine, built on **.NET 8** with Clean / Hexagonal Architecture.

> This is one language port within a [polyglot monorepo](../README.md). The
> shared cross-language conformance payloads live in [`../samples`](../samples)
> and the deployment manifests in [`../deploy`](../deploy).

## Layout

```
dotnet/
├── FhirOpenEhrBridge.sln
├── Directory.Build.props
├── src/
│   ├── FhirOpenEhrBridge.Domain          # contracts + openEHR mapping models (no deps)
│   ├── FhirOpenEhrBridge.Application      # mappers, validation, TranslationService (Firely)
│   ├── FhirOpenEhrBridge.Infrastructure   # FHIR server / openEHR CDR HTTP adapters
│   └── FhirOpenEhrBridge.Api              # ASP.NET Core Web API (composition root)
├── tests/
│   ├── FhirOpenEhrBridge.UnitTests        # xUnit + Moq
│   ├── FhirOpenEhrBridge.BddTests         # Reqnroll (Gherkin)
│   └── FhirOpenEhrBridge.IntegrationTests # WebApplicationFactory
└── samples/FhirOpenEhrBridge.Demo         # console demo over ../../samples
```

## Common commands

```bash
# from this dotnet/ directory
dotnet restore
dotnet build
dotnet test
dotnet run --project src/FhirOpenEhrBridge.Api          # API + Swagger
dotnet run --project samples/FhirOpenEhrBridge.Demo     # console capability demo
```

See the [root README](../README.md) for full architecture, API, and deployment
documentation.
