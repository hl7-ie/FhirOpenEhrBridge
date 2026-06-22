using FhirOpenEhrBridge.Application.Mapping.Patient;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.Domain.Validation;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Reqnroll;
using Xunit;

namespace FhirOpenEhrBridge.BddTests.StepDefinitions;

/// <summary>
/// Step bindings for both translation features. A new instance is created per
/// scenario, so the instance fields safely hold per-scenario state.
/// </summary>
[Binding]
public sealed class TranslationStepDefinitions
{
    private readonly PatientToDemographicsMapper _fhirToOpenEhr = new();
    private readonly DemographicsToPatientMapper _openEhrToFhir = new();

    private string? _fhirJson;
    private OpenEhrComposition? _composition;
    private OpenEhrComposition? _producedComposition;
    private string? _producedFhir;

    private bool _succeeded;
    private IReadOnlyList<ValidationIssue> _issues = Array.Empty<ValidationIssue>();

    private const string ValidPatientJson =
        """
        {
          "resourceType": "Patient",
          "id": "example-123",
          "name": [ { "use": "official", "family": "Smith", "given": ["John"] } ],
          "gender": "male",
          "birthDate": "1980-05-15"
        }
        """;

    // --- Given -------------------------------------------------------------

    [Given("a valid FHIR Patient resource")]
    public void GivenAValidFhirPatientResource() => _fhirJson = ValidPatientJson;

    [Given("a FHIR Observation resource")]
    public void GivenAFhirObservationResource() =>
        _fhirJson = """{ "resourceType": "Observation", "status": "final" }""";

    [Given("a valid openEHR demographics composition")]
    public void GivenAValidOpenEhrDemographicsComposition() =>
        _composition = new OpenEhrComposition
        {
            EhrStatus = new OpenEhrEhrStatus { SubjectId = "example-123" },
            Demographics = new OpenEhrDemographics
            {
                FamilyName = "Smith",
                GivenName = "John",
                Gender = "male",
                BirthDate = "1980-05-15"
            }
        };

    [Given("an openEHR composition with no demographics")]
    public void GivenAnOpenEhrCompositionWithNoDemographics() =>
        _composition = new OpenEhrComposition { Demographics = null! };

    // --- When --------------------------------------------------------------

    [When("I translate the FHIR resource to openEHR")]
    public void WhenITranslateTheFhirResourceToOpenEhr()
    {
        var result = _fhirToOpenEhr.Map(_fhirJson!);
        _producedComposition = result.Value;
        _succeeded = result.Succeeded;
        _issues = result.Issues;
    }

    [When("I translate the openEHR composition to FHIR")]
    public void WhenITranslateTheOpenEhrCompositionToFhir()
    {
        var result = _openEhrToFhir.Map(_composition!);
        _producedFhir = result.Value;
        _succeeded = result.Succeeded;
        _issues = result.Issues;
    }

    // --- Then --------------------------------------------------------------

    [Then("the translation succeeds")]
    public void ThenTheTranslationSucceeds() => Assert.True(_succeeded);

    [Then("the translation fails")]
    public void ThenTheTranslationFails() => Assert.False(_succeeded);

    [Then("there is a validation error")]
    public void ThenThereIsAValidationError() =>
        Assert.Contains(_issues, i => i.Severity == ValidationSeverity.Error);

    [Then("the openEHR composition has archetype {string}")]
    public void ThenTheOpenEhrCompositionHasArchetype(string archetype) =>
        Assert.Equal(archetype, _producedComposition!.ArchetypeNodeId);

    [Then("the demographics family name is {string}")]
    public void ThenTheDemographicsFamilyNameIs(string familyName) =>
        Assert.Equal(familyName, _producedComposition!.Demographics.FamilyName);

    [Then("the demographics given name is {string}")]
    public void ThenTheDemographicsGivenNameIs(string givenName) =>
        Assert.Equal(givenName, _producedComposition!.Demographics.GivenName);

    [Then("the result is a FHIR Bundle")]
    public void ThenTheResultIsAFhirBundle()
    {
        var bundle = new FhirJsonParser().Parse<Bundle>(_producedFhir!);
        Assert.Equal(Bundle.BundleType.Collection, bundle.Type);
    }

    [Then("the Bundle contains a Patient with family name {string}")]
    public void ThenTheBundleContainsAPatientWithFamilyName(string familyName)
    {
        var bundle = new FhirJsonParser().Parse<Bundle>(_producedFhir!);
        var patient = (Patient)bundle.Entry.Single().Resource;
        Assert.Equal(familyName, patient.Name[0].Family);
    }
}
