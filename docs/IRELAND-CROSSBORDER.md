# Ireland context & cross-border scopes

These examples ground the translation engine in real Irish and cross-border
healthcare scenarios. They live under [`samples/`](../samples) and run through
the standard pipeline — the engine maps **multiple national identifiers** in a
single record, which is exactly what cross-border interoperability needs.

> **⚠️ Identifier systems are placeholders — do not treat them as canonical.**
> The Irish identifier `system` URIs in these samples use the RFC 2606 reserved
> documentation domain `http://example.org/ie/...`. They are deliberately
> non-authoritative.
>
> **Do NOT use `hl7.ie` or `fhir.ie`.** Those domains are *not* operated by HL7
> International, its Irish affiliate, or the HSE; they have been used by third
> parties for misleading/illegitimate "implementations". The canonical Irish
> identifier systems must be obtained from the official HSE / Department of
> Health publications before any real use.
>
> All patient details and identifier values here are fictional.

## Identifier systems used

| Identifier | Who | `system` used in samples (placeholder) | Notes |
| --- | --- | --- | --- |
| **IHI** — Individual Health Identifier | Republic of Ireland (HSE) | `http://example.org/ie/individual-health-identifier` | National patient identifier (Health Identifiers Act 2014). Use the official HSE-published system in production. |
| **PPSN** — Personal Public Service Number | Republic of Ireland | `http://example.org/ie/pps-number` | Social identifier, occasionally used as a secondary reference. |
| **NHS number** | Northern Ireland / UK | `https://fhir.nhs.uk/Id/nhs-number` | 10-digit; canonical UK system, used for all-island care. |

Irish addresses use **Eircode** postal codes (e.g. `D04 X2P3`, `H91 RX9F`) and
country `IE`; Northern Ireland addresses use country `GB`.

## Scenarios

### 1. Domestic — Irish patient with an IHI
- FHIR: [`samples/fhir/ie-patient-ihi.json`](../samples/fhir/ie-patient-ihi.json)
- openEHR: [`samples/openehr/ie-composition-ihi.json`](../samples/openehr/ie-composition-ihi.json)

A Dublin-resident patient identified by their IHI. Demonstrates the baseline
HSE demographics mapping, Eircode handling, and Irish-language names (the
`Ó Briain` surname round-trips intact through UTF-8).

### 2. All-island care — Republic of Ireland ↔ Northern Ireland
- FHIR: [`samples/fhir/ie-patient-crossborder-ni.json`](../samples/fhir/ie-patient-crossborder-ni.json)
- openEHR: [`samples/openehr/ie-composition-crossborder.json`](../samples/openehr/ie-composition-crossborder.json)

A patient resident in Derry (Northern Ireland, `GB`) who also holds a Republic
of Ireland **IHI** — typical of cross-border services (e.g. the all-island
congenital heart disease or North West Cancer Centre pathways). The record
carries **both** the IHI and the NI **NHS number**; the engine maps both
identifiers in each direction, so neither jurisdiction's reference is lost.

### 3. EU cross-border — MyHealth@EU / International Patient Summary
- FHIR: [`samples/fhir/ie-patient-ips-eu.json`](../samples/fhir/ie-patient-ips-eu.json)

An Irish resident whose Patient Summary may be shared via **MyHealth@EU**
(eHDSI) when they seek unscheduled care in another EU member state. The Patient
claims conformance to the HL7 **International Patient Summary (IPS)** profile and
carries both the IHI and PPSN. Translating to openEHR demographics shows how a
national CDR can ingest an inbound IPS subject, and back to FHIR shows producing
an IPS-aligned `Patient` for outbound exchange.

## Try it

```bash
# All samples (including the Irish ones) through the engine, both directions:
dotnet run --project samples/FhirOpenEhrBridge.Demo

# Against the live API:
curl -X POST http://localhost:5199/api/translate/fhir-to-openehr \
  -H "Content-Type: application/fhir+json" \
  --data @samples/fhir/ie-patient-crossborder-ni.json
```

See also [`samples/requests-ireland.http`](../samples/requests-ireland.http).
