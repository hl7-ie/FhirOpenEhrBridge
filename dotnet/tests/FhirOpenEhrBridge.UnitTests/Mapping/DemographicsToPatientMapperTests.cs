using FhirOpenEhrBridge.Application.Mapping.Patient;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.UnitTests.TestData;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Xunit;

namespace FhirOpenEhrBridge.UnitTests.Mapping;

public sealed class DemographicsToPatientMapperTests
{
    private readonly DemographicsToPatientMapper _mapper = new();
    private readonly FhirJsonParser _parser = new();

    [Fact]
    public void Map_ValidComposition_ProducesFhirBundleWithPatient()
    {
        var composition = SamplePayloads.BuildDemographicsComposition();

        var result = _mapper.Map(composition);

        Assert.True(result.Succeeded);
        var bundle = _parser.Parse<Bundle>(result.Value!);
        Assert.Equal(Bundle.BundleType.Collection, bundle.Type);

        var patient = Assert.IsType<Patient>(Assert.Single(bundle.Entry).Resource);
        Assert.Equal("example-123", patient.Id);
        Assert.Equal("Smith", patient.Name[0].Family);
        Assert.Equal("John", patient.Name[0].Given.First());
        Assert.Equal(AdministrativeGender.Male, patient.Gender);
        Assert.Equal("1980-05-15", patient.BirthDate);
    }

    [Fact]
    public void Map_ValidComposition_MapsIdentifierAndAddress()
    {
        var composition = SamplePayloads.BuildDemographicsComposition();

        var result = _mapper.Map(composition);

        var bundle = _parser.Parse<Bundle>(result.Value!);
        var patient = (Patient)bundle.Entry.Single().Resource;

        Assert.Equal("9876543210", patient.Identifier[0].Value);
        Assert.Equal("https://fhir.nhs.uk/Id/nhs-number", patient.Identifier[0].System);
        Assert.Equal("London", patient.Address[0].City);
        Assert.Equal("10 Downing Street", patient.Address[0].Line.First());
    }

    [Fact]
    public void Map_NullComposition_Fails()
    {
        var result = _mapper.Map(null!);

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Issues);
    }

    [Fact]
    public void Map_MissingArchetype_Fails()
    {
        var composition = SamplePayloads.BuildDemographicsComposition();
        composition.ArchetypeNodeId = string.Empty;

        var result = _mapper.Map(composition);

        Assert.False(result.Succeeded);
    }
}
