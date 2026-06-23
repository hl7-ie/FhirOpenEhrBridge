package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.node.ArrayNode;
import com.fasterxml.jackson.databind.node.ObjectNode;
import java.time.OffsetDateTime;
import java.time.ZoneOffset;
import java.util.List;
import java.util.UUID;

/** Bidirectional mappers. */
public final class Mappers {
    private Mappers() {}

    static final String ARCHETYPE = "openEHR-EHR-COMPOSITION.demographics.v1";

    /** FHIR Patient JSON -> openEHR demographics composition. */
    public static TranslationResult<JsonNode> fhirPatientToComposition(String fhirJson) {
        Validation.FhirParse parse = Validation.validateFhirPatientJson(fhirJson);
        if (Validation.hasError(parse.issues()) || parse.patient() == null) {
            return TranslationResult.fail(parse.issues());
        }
        JsonNode p = parse.patient();

        ObjectNode comp = Json.MAPPER.createObjectNode();
        comp.put("archetypeNodeId", ARCHETYPE);
        comp.put("name", "Demographics");
        comp.put("language", "en");
        comp.put("territory", "GB");
        comp.put("category", "persistent");
        comp.put("startTime", OffsetDateTime.now(ZoneOffset.UTC).toString());

        ObjectNode ehr = comp.putObject("ehrStatus");
        String id = p.path("id").asText("");
        ehr.put("subjectId", id.isBlank() ? UUID.randomUUID().toString() : id);
        ehr.put("subjectNamespace", "DEMOGRAPHIC");
        ehr.put("isQueryable", true);
        ehr.put("isModifiable", true);

        ObjectNode demo = comp.putObject("demographics");
        JsonNode name0 = p.path("name").path(0);
        if (name0.hasNonNull("family")) {
            demo.put("familyName", name0.path("family").asText());
        }
        JsonNode given = name0.path("given");
        if (given.isArray() && !given.isEmpty()) {
            demo.put("givenName", given.path(0).asText());
        }
        demo.put("gender", GenderMap.toOpenEhr(p.path("gender").asText(null)));
        if (p.hasNonNull("birthDate")) {
            demo.put("birthDate", p.path("birthDate").asText());
        }

        ArrayNode identifiers = demo.putArray("identifiers");
        for (JsonNode idn : p.path("identifier")) {
            String value = idn.path("value").asText("");
            if (value.isBlank()) {
                continue;
            }
            ObjectNode oi = identifiers.addObject();
            oi.put("id", value);
            if (idn.hasNonNull("system")) {
                oi.put("issuer", idn.path("system").asText());
            }
            JsonNode type = idn.path("type");
            String typeText = type.path("text").asText("");
            if (typeText.isBlank()) {
                typeText = type.path("coding").path(0).path("code").asText("");
            }
            if (!typeText.isBlank()) {
                oi.put("type", typeText);
            }
        }

        JsonNode addr0 = p.path("address").path(0);
        if (addr0.isObject()) {
            ObjectNode a = demo.putObject("address");
            JsonNode line = addr0.path("line");
            if (line.isArray() && !line.isEmpty()) {
                a.put("line", line.path(0).asText());
            }
            if (addr0.hasNonNull("city")) {
                a.put("city", addr0.path("city").asText());
            }
            if (addr0.hasNonNull("postalCode")) {
                a.put("postalCode", addr0.path("postalCode").asText());
            }
            if (addr0.hasNonNull("country")) {
                a.put("country", addr0.path("country").asText());
            }
        }

        return TranslationResult.ok(comp, parse.issues());
    }

    /** openEHR demographics composition -> FHIR Bundle. */
    public static TranslationResult<JsonNode> compositionToFhirBundle(JsonNode comp) {
        List<ValidationIssue> issues = Validation.validateComposition(comp);
        if (Validation.hasError(issues)) {
            return TranslationResult.fail(issues);
        }
        JsonNode d = comp.path("demographics");

        String patientId = comp.path("ehrStatus").path("subjectId").asText("");
        if (patientId.isBlank()) {
            patientId = UUID.randomUUID().toString();
        }

        ObjectNode patient = Json.MAPPER.createObjectNode();
        patient.put("resourceType", "Patient");
        patient.put("id", patientId);

        String family = d.path("familyName").asText("");
        String givenName = d.path("givenName").asText("");
        if (!family.isBlank() || !givenName.isBlank()) {
            ObjectNode name = patient.putArray("name").addObject();
            name.put("use", "official");
            if (!family.isBlank()) {
                name.put("family", family);
            }
            if (!givenName.isBlank()) {
                name.putArray("given").add(givenName);
            }
        }

        String gender = GenderMap.toFhir(d.path("gender").asText(null));
        if (gender != null) {
            patient.put("gender", gender);
        }
        String birthDate = d.path("birthDate").asText("");
        if (!birthDate.isBlank()) {
            patient.put("birthDate", birthDate);
        }

        ArrayNode outIds = Json.MAPPER.createArrayNode();
        for (JsonNode i : d.path("identifiers")) {
            String idv = i.path("id").asText("");
            if (idv.isBlank()) {
                continue;
            }
            ObjectNode fi = outIds.addObject();
            if (i.hasNonNull("issuer")) {
                fi.put("system", i.path("issuer").asText());
            }
            fi.put("value", idv);
            String t = i.path("type").asText("");
            if (!t.isBlank()) {
                fi.putObject("type").put("text", t);
            }
        }
        if (!outIds.isEmpty()) {
            patient.set("identifier", outIds);
        }

        JsonNode addr = d.path("address");
        if (addr.isObject()) {
            ObjectNode a = patient.putArray("address").addObject();
            String line = addr.path("line").asText("");
            if (!line.isBlank()) {
                a.putArray("line").add(line);
            }
            String city = addr.path("city").asText("");
            if (!city.isBlank()) {
                a.put("city", city);
            }
            String postalCode = addr.path("postalCode").asText("");
            if (!postalCode.isBlank()) {
                a.put("postalCode", postalCode);
            }
            String country = addr.path("country").asText("");
            if (!country.isBlank()) {
                a.put("country", country);
            }
        }

        ObjectNode bundle = Json.MAPPER.createObjectNode();
        bundle.put("resourceType", "Bundle");
        bundle.put("id", UUID.randomUUID().toString());
        bundle.put("type", "collection");
        String ts = comp.path("startTime").asText("");
        if (!ts.isBlank()) {
            bundle.put("timestamp", ts);
        }
        ObjectNode entry = bundle.putArray("entry").addObject();
        entry.put("fullUrl", "urn:uuid:" + patientId);
        entry.set("resource", patient);

        return TranslationResult.ok(bundle, issues);
    }
}
