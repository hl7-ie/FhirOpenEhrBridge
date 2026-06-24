package io.github.hl7ie.fhiropenehrbridge;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.fasterxml.jackson.databind.JsonNode;
import io.cucumber.java.Before;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;

public class StepDefinitions {

    private final TranslationService service = new TranslationService();
    private String fhirJson;
    private JsonNode composition;
    private TranslationResult<JsonNode> fhirResult;
    private TranslationResult<JsonNode> bundleResult;

    @Before
    public void reset() {
        fhirJson = null;
        composition = null;
        fhirResult = null;
        bundleResult = null;
    }

    @Given("a valid FHIR Patient JSON")
    public void aValidFhirPatientJson() {
        fhirJson = "{\"resourceType\":\"Patient\",\"id\":\"bdd-1\","
            + "\"name\":[{\"family\":\"Smith\",\"given\":[\"John\"]}],"
            + "\"gender\":\"male\",\"birthDate\":\"1980-05-15\"}";
    }

    @Given("a FHIR Observation JSON")
    public void aFhirObservationJson() {
        fhirJson = "{\"resourceType\":\"Observation\",\"status\":\"final\"}";
    }

    @Given("a valid openEHR demographics composition")
    public void aValidComposition() throws Exception {
        composition = Json.MAPPER.readTree("{\"archetypeNodeId\":\"openEHR-EHR-COMPOSITION.demographics.v1\","
            + "\"ehrStatus\":{\"subjectId\":\"bdd-2\"},"
            + "\"demographics\":{\"familyName\":\"Smith\",\"givenName\":\"John\",\"gender\":\"male\"}}");
    }

    @Given("an openEHR composition without demographics")
    public void aCompositionWithoutDemographics() throws Exception {
        composition = Json.MAPPER.readTree("{\"archetypeNodeId\":\"openEHR-EHR-COMPOSITION.demographics.v1\","
            + "\"ehrStatus\":{\"subjectId\":\"bdd-3\"},\"demographics\":null}");
    }

    @When("I translate it to openEHR")
    public void translateToOpenEhr() {
        fhirResult = service.fhirToOpenEhr(fhirJson);
    }

    @When("I translate it to FHIR")
    public void translateToFhir() {
        bundleResult = service.openEhrToFhir(composition);
    }

    @Then("the translation succeeds")
    public void translationSucceeds() {
        assertTrue(succeeded());
    }

    @Then("the translation fails")
    public void translationFails() {
        assertFalse(succeeded());
    }

    @Then("the openEHR demographics family name is {string}")
    public void familyNameIs(String expected) {
        assertEquals(expected, fhirResult.value().path("demographics").path("familyName").asText());
    }

    @Then("the result is a FHIR Bundle containing a Patient")
    public void resultIsBundleWithPatient() {
        JsonNode bundle = bundleResult.value();
        assertEquals("Bundle", bundle.path("resourceType").asText());
        assertEquals("Patient", bundle.path("entry").path(0).path("resource").path("resourceType").asText());
    }

    private boolean succeeded() {
        return fhirResult != null ? fhirResult.success() : bundleResult.success();
    }
}
