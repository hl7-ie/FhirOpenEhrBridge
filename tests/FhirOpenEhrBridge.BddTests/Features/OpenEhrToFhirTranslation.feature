Feature: openEHR to FHIR translation
    As an integration engine
    I want to translate openEHR demographics compositions into FHIR
    So that openEHR-native systems can expose data to FHIR consumers

Scenario: Translating a valid openEHR Composition produces a FHIR Bundle
    Given a valid openEHR demographics composition
    When I translate the openEHR composition to FHIR
    Then the translation succeeds
    And the result is a FHIR Bundle
    And the Bundle contains a Patient with family name "Smith"

Scenario: Translating a composition without a demographics payload is rejected
    Given an openEHR composition with no demographics
    When I translate the openEHR composition to FHIR
    Then the translation fails
    And there is a validation error
