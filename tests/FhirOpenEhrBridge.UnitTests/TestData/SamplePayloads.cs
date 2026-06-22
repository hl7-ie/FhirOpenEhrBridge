using FhirOpenEhrBridge.Domain.Models.OpenEhr;

namespace FhirOpenEhrBridge.UnitTests.TestData;

/// <summary>Reusable sample payloads for the mapping unit tests.</summary>
internal static class SamplePayloads
{
    /// <summary>A valid FHIR R4 <c>Patient</c> resource as JSON.</summary>
    public const string PatientJson =
        """
        {
          "resourceType": "Patient",
          "id": "example-123",
          "identifier": [
            {
              "system": "https://fhir.nhs.uk/Id/nhs-number",
              "value": "9876543210",
              "type": { "text": "NHS" }
            }
          ],
          "name": [
            { "use": "official", "family": "Smith", "given": ["John"] }
          ],
          "gender": "male",
          "birthDate": "1980-05-15",
          "address": [
            {
              "line": ["10 Downing Street"],
              "city": "London",
              "postalCode": "SW1A 2AA",
              "country": "GB"
            }
          ]
        }
        """;

    /// <summary>A FHIR <c>Observation</c> — used to assert that the Patient mapper rejects it.</summary>
    public const string ObservationJson =
        """
        { "resourceType": "Observation", "status": "final", "code": { "text": "Heart rate" } }
        """;

    /// <summary>Builds a valid openEHR demographics composition equivalent to <see cref="PatientJson"/>.</summary>
    public static OpenEhrComposition BuildDemographicsComposition() => new()
    {
        ArchetypeNodeId = "openEHR-EHR-COMPOSITION.demographics.v1",
        EhrStatus = new OpenEhrEhrStatus { SubjectId = "example-123" },
        Demographics = new OpenEhrDemographics
        {
            FamilyName = "Smith",
            GivenName = "John",
            Gender = "male",
            BirthDate = "1980-05-15",
            Identifiers =
            {
                new OpenEhrIdentifier
                {
                    Id = "9876543210",
                    Issuer = "https://fhir.nhs.uk/Id/nhs-number",
                    Type = "NHS"
                }
            },
            Address = new OpenEhrAddress
            {
                Line = "10 Downing Street",
                City = "London",
                PostalCode = "SW1A 2AA",
                Country = "GB"
            }
        }
    };
}
