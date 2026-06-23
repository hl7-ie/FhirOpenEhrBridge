using FhirOpenEhrBridge.Application.Mapping.Patient;
using FhirOpenEhrBridge.UnitTests.TestData;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Xunit;

namespace FhirOpenEhrBridge.UnitTests.Mapping;

/// <summary>
/// Verifies that a FHIR Patient survives a round trip
/// (FHIR &#8594; openEHR &#8594; FHIR) without losing its core demographic data.
/// </summary>
public sealed class PatientRoundTripTests
{
    [Fact]
    public void Patient_SurvivesRoundTrip()
    {
        var toOpenEhr = new PatientToDemographicsMapper();
        var toFhir = new DemographicsToPatientMapper();

        var composition = toOpenEhr.Map(SamplePayloads.PatientJson);
        Assert.True(composition.Succeeded);

        var fhir = toFhir.Map(composition.Value!);
        Assert.True(fhir.Succeeded);

        var bundle = new FhirJsonParser().Parse<Bundle>(fhir.Value!);
        var patient = (Patient)bundle.Entry.Single().Resource;

        Assert.Equal("example-123", patient.Id);
        Assert.Equal("Smith", patient.Name[0].Family);
        Assert.Equal("John", patient.Name[0].Given.First());
        Assert.Equal(AdministrativeGender.Male, patient.Gender);
        Assert.Equal("1980-05-15", patient.BirthDate);
        Assert.Equal("9876543210", patient.Identifier[0].Value);
    }
}
