Feature: FHIR to openEHR translation

  Scenario: Translating a valid FHIR Patient
    Given a valid FHIR Patient JSON
    When I translate it to openEHR
    Then the translation succeeds
    And the openEHR demographics family name is "Smith"

  Scenario: Rejecting a non-Patient resource
    Given a FHIR Observation JSON
    When I translate it to openEHR
    Then the translation fails
