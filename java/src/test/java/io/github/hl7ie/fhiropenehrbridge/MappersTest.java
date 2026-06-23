package io.github.hl7ie.fhiropenehrbridge;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.fasterxml.jackson.databind.JsonNode;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

class MappersTest {

    private static final String PATIENT_JSON = """
        {
          "resourceType": "Patient", "id": "example-123",
          "identifier": [{ "system": "https://fhir.nhs.uk/Id/nhs-number", "value": "9876543210", "type": { "text": "NHS" } }],
          "name": [{ "family": "Smith", "given": ["John"] }],
          "gender": "male", "birthDate": "1980-05-15",
          "address": [{ "line": ["10 Downing Street"], "city": "London", "postalCode": "SW1A 2AA", "country": "GB" }]
        }""";

    private static JsonNode sampleComposition() throws Exception {
        return Json.MAPPER.readTree("""
            {
              "archetypeNodeId": "openEHR-EHR-COMPOSITION.demographics.v1",
              "ehrStatus": { "subjectId": "example-123" },
              "demographics": {
                "familyName": "Smith", "givenName": "John", "gender": "male", "birthDate": "1980-05-15",
                "identifiers": [{ "id": "9876543210", "issuer": "https://fhir.nhs.uk/Id/nhs-number", "type": "NHS" }],
                "address": { "line": "10 Downing Street", "city": "London", "postalCode": "SW1A 2AA", "country": "GB" }
              }
            }""");
    }

    @Test
    void mapsValidPatient() {
        TranslationResult<JsonNode> result = Mappers.fhirPatientToComposition(PATIENT_JSON);
        assertTrue(result.success());
        JsonNode d = result.value().path("demographics");
        assertEquals("Smith", d.path("familyName").asText());
        assertEquals("John", d.path("givenName").asText());
        assertEquals("male", d.path("gender").asText());
        assertEquals("1980-05-15", d.path("birthDate").asText());
        assertEquals("example-123", result.value().path("ehrStatus").path("subjectId").asText());
        assertEquals("9876543210", d.path("identifiers").path(0).path("id").asText());
        assertEquals("London", d.path("address").path("city").asText());
    }

    @Test
    void rejectsNonPatient() {
        TranslationResult<JsonNode> result =
            Mappers.fhirPatientToComposition("{\"resourceType\":\"Observation\",\"status\":\"final\"}");
        assertFalse(result.success());
        assertTrue(Validation.hasError(result.issues()));
    }

    @ParameterizedTest
    @ValueSource(strings = {"", "   ", "{ not json"})
    void rejectsInvalidJson(String bad) {
        assertFalse(Mappers.fhirPatientToComposition(bad).success());
    }

    @Test
    void mapsCompositionToBundle() throws Exception {
        TranslationResult<JsonNode> result = Mappers.compositionToFhirBundle(sampleComposition());
        assertTrue(result.success());
        JsonNode bundle = result.value();
        assertEquals("Bundle", bundle.path("resourceType").asText());
        assertEquals("collection", bundle.path("type").asText());
        JsonNode patient = bundle.path("entry").path(0).path("resource");
        assertEquals("example-123", patient.path("id").asText());
        assertEquals("male", patient.path("gender").asText());
        assertEquals("9876543210", patient.path("identifier").path(0).path("value").asText());
    }

    @Test
    void rejectsMissingDemographics() throws Exception {
        com.fasterxml.jackson.databind.node.ObjectNode comp =
            (com.fasterxml.jackson.databind.node.ObjectNode) sampleComposition();
        comp.remove("demographics");
        assertFalse(Mappers.compositionToFhirBundle(comp).success());
    }

    @Test
    void roundTripPreservesCore() {
        JsonNode comp = Mappers.fhirPatientToComposition(PATIENT_JSON).value();
        JsonNode bundle = Mappers.compositionToFhirBundle(comp).value();
        JsonNode patient = bundle.path("entry").path(0).path("resource");
        assertEquals("example-123", patient.path("id").asText());
        assertEquals("Smith", patient.path("name").path(0).path("family").asText());
        assertEquals("9876543210", patient.path("identifier").path(0).path("value").asText());
    }
}
