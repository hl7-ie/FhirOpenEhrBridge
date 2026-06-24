package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;
import java.util.ArrayList;
import java.util.List;

/** Inbound payload validation. */
public final class Validation {
    private Validation() {}

    /** Result of parsing inbound FHIR JSON: any issues plus the parsed node (or null). */
    public record FhirParse(List<ValidationIssue> issues, JsonNode patient) {}

    public static FhirParse validateFhirPatientJson(String json) {
        List<ValidationIssue> issues = new ArrayList<>();
        if (json == null || json.isBlank()) {
            issues.add(ValidationIssue.error("FHIR payload is empty.", null));
            return new FhirParse(issues, null);
        }
        JsonNode node;
        try {
            node = Json.MAPPER.readTree(json);
        } catch (Exception e) {
            issues.add(ValidationIssue.error("Payload is not valid FHIR JSON: " + e.getMessage(), null));
            return new FhirParse(issues, null);
        }
        if (node == null || !node.isObject()) {
            issues.add(ValidationIssue.error("Payload did not parse to a FHIR resource.", null));
            return new FhirParse(issues, null);
        }
        String resourceType = node.path("resourceType").asText("");
        if (!"Patient".equals(resourceType)) {
            issues.add(ValidationIssue.error(
                "Expected a FHIR 'Patient' resource but received '" + resourceType + "'.", "resourceType"));
            return new FhirParse(issues, null);
        }
        return new FhirParse(issues, node);
    }

    public static List<ValidationIssue> validateComposition(JsonNode c) {
        List<ValidationIssue> issues = new ArrayList<>();
        if (c == null || c.isNull()) {
            issues.add(ValidationIssue.error("openEHR composition is null.", null));
            return issues;
        }
        if (c.path("archetypeNodeId").asText("").isBlank()) {
            issues.add(ValidationIssue.error("Composition is missing 'archetypeNodeId'.", "archetypeNodeId"));
        }
        JsonNode demographics = c.get("demographics");
        if (demographics == null || demographics.isNull()) {
            issues.add(ValidationIssue.error("Composition does not contain a demographics payload.", "demographics"));
        } else if (demographics.path("familyName").asText("").isBlank()
                && demographics.path("givenName").asText("").isBlank()) {
            issues.add(ValidationIssue.warning(
                "Demographics contain neither a family name nor a given name.", "demographics.familyName"));
        }
        if (c.path("ehrStatus").path("subjectId").asText("").isBlank()) {
            issues.add(ValidationIssue.warning(
                "EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.",
                "ehrStatus.subjectId"));
        }
        return issues;
    }

    public static boolean hasError(List<ValidationIssue> issues) {
        return issues.stream().anyMatch(i -> "error".equals(i.severity()));
    }
}
