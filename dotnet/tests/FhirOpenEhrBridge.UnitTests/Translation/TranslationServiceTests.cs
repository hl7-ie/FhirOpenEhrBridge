using FhirOpenEhrBridge.Application.Translation;
using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.UnitTests.TestData;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FhirOpenEhrBridge.UnitTests.Translation;

public sealed class TranslationServiceTests
{
    [Fact]
    public void FhirToOpenEhr_DispatchesToMapperMatchingResourceType()
    {
        // A Moq-backed mapper that claims to support "Patient".
        var expected = new OpenEhrComposition();
        var mapper = new Mock<IFhirToOpenEhrMapper<OpenEhrComposition>>();
        mapper.SetupGet(m => m.SupportedFhirResourceType).Returns("Patient");
        mapper.Setup(m => m.Map(It.IsAny<string>()))
              .Returns(MappingResult<OpenEhrComposition>.Ok(expected));

        var service = new TranslationService(
            new[] { mapper.Object },
            Array.Empty<IOpenEhrToFhirMapper>(),
            NullLogger<TranslationService>.Instance);

        var result = service.FhirToOpenEhr(SamplePayloads.PatientJson);

        Assert.True(result.Succeeded);
        Assert.Same(expected, result.Value);
        mapper.Verify(m => m.Map(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void FhirToOpenEhr_NoMapperForResourceType_Fails()
    {
        var service = new TranslationService(
            Array.Empty<IFhirToOpenEhrMapper>(),
            Array.Empty<IOpenEhrToFhirMapper>(),
            NullLogger<TranslationService>.Instance);

        var result = service.FhirToOpenEhr(SamplePayloads.PatientJson);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Issues, i => i.Message.Contains("No FHIR-to-openEHR mapper"));
    }

    [Fact]
    public void OpenEhrToFhir_DispatchesToMapperMatchingArchetype()
    {
        var mapper = new Mock<IOpenEhrToFhirMapper<OpenEhrComposition>>();
        mapper.SetupGet(m => m.SupportedArchetypeId).Returns("openEHR-EHR-COMPOSITION.demographics.v1");
        mapper.Setup(m => m.Map(It.IsAny<OpenEhrComposition>()))
              .Returns(MappingResult<string>.Ok("{\"resourceType\":\"Bundle\"}"));

        var service = new TranslationService(
            Array.Empty<IFhirToOpenEhrMapper>(),
            new[] { mapper.Object },
            NullLogger<TranslationService>.Instance);

        var result = service.OpenEhrToFhir(SamplePayloads.BuildDemographicsComposition());

        Assert.True(result.Succeeded);
        mapper.Verify(m => m.Map(It.IsAny<OpenEhrComposition>()), Times.Once);
    }

    [Fact]
    public void FhirToOpenEhr_InvalidJson_Fails()
    {
        var service = new TranslationService(
            Array.Empty<IFhirToOpenEhrMapper>(),
            Array.Empty<IOpenEhrToFhirMapper>(),
            NullLogger<TranslationService>.Instance);

        var result = service.FhirToOpenEhr("{ not valid json");

        Assert.False(result.Succeeded);
    }
}
