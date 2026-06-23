using FhirOpenEhrBridge.Application.Mapping.Patient;
using FhirOpenEhrBridge.Domain.Validation;
using FhirOpenEhrBridge.UnitTests.TestData;
using Xunit;

namespace FhirOpenEhrBridge.UnitTests.Mapping;

public sealed class PatientToDemographicsMapperTests
{
    private readonly PatientToDemographicsMapper _mapper = new();

    [Fact]
    public void Map_ValidPatient_ProducesDemographics()
    {
        var result = _mapper.Map(SamplePayloads.PatientJson);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);

        var demographics = result.Value!.Demographics;
        Assert.Equal("Smith", demographics.FamilyName);
        Assert.Equal("John", demographics.GivenName);
        Assert.Equal("male", demographics.Gender);
        Assert.Equal("1980-05-15", demographics.BirthDate);
    }

    [Fact]
    public void Map_ValidPatient_MapsIdentifierAndAddress()
    {
        var result = _mapper.Map(SamplePayloads.PatientJson);

        var demographics = result.Value!.Demographics;
        var identifier = Assert.Single(demographics.Identifiers);
        Assert.Equal("9876543210", identifier.Id);
        Assert.Equal("https://fhir.nhs.uk/Id/nhs-number", identifier.Issuer);
        Assert.Equal("NHS", identifier.Type);

        Assert.NotNull(demographics.Address);
        Assert.Equal("10 Downing Street", demographics.Address!.Line);
        Assert.Equal("London", demographics.Address.City);
        Assert.Equal("SW1A 2AA", demographics.Address.PostalCode);
        Assert.Equal("GB", demographics.Address.Country);
    }

    [Fact]
    public void Map_ValidPatient_CopiesIdIntoEhrStatusSubject()
    {
        var result = _mapper.Map(SamplePayloads.PatientJson);

        Assert.Equal("example-123", result.Value!.EhrStatus.SubjectId);
        Assert.Equal("DEMOGRAPHIC", result.Value.EhrStatus.SubjectNamespace);
    }

    [Fact]
    public void SupportedFhirResourceType_IsPatient()
    {
        Assert.Equal("Patient", _mapper.SupportedFhirResourceType);
    }

    [Fact]
    public void Map_WrongResourceType_FailsWithError()
    {
        var result = _mapper.Map(SamplePayloads.ObservationJson);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Issues, i => i.Severity == ValidationSeverity.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ not json")]
    public void Map_InvalidJson_FailsGracefully(string payload)
    {
        var result = _mapper.Map(payload);

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Issues);
    }
}
