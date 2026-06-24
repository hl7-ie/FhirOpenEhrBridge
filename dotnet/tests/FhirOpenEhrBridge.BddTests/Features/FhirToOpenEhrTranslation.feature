Feature: FHIR to openEHR translation
    As an integration engine
    I want to translate FHIR Patient resources into openEHR demographics
    So that FHIR-native systems can populate an openEHR CDR

Scenario: Translating a valid FHIR Patient produces an openEHR demographics composition
    Given a valid FHIR Patient resource
    When I translate the FHIR resource to openEHR
    Then the translation succeeds
    And the openEHR composition has archetype "openEHR-EHR-COMPOSITION.demographics.v1"
    And the demographics family name is "Smith"
    And the demographics given name is "John"

Scenario: Translating a non-Patient resource is rejected
    Given a FHIR Observation resource
    When I translate the FHIR resource to openEHR
    Then the translation fails
    And there is a validation error
