Feature: openEHR to FHIR translation

  Scenario: Translating a valid composition
    Given a valid openEHR demographics composition
    When I translate it to FHIR
    Then the translation succeeds
    And the result is a FHIR Bundle containing a Patient

  Scenario: Rejecting a composition without demographics
    Given an openEHR composition without demographics
    When I translate it to FHIR
    Then the translation fails
