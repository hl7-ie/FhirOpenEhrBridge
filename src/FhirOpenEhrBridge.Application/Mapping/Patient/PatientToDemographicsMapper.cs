using FhirOpenEhrBridge.Application.Fhir;
using FhirOpenEhrBridge.Application.Validation;
using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.Domain.Validation;
using Hl7.Fhir.Model;

namespace FhirOpenEhrBridge.Application.Mapping.Patient;

/// <summary>
/// Proof-of-concept mapper translating a FHIR <c>Patient</c> resource into an
/// openEHR demographics <see cref="OpenEhrComposition"/> (carrying an
/// <c>EHR_STATUS</c> subject reference and a <c>PERSON</c> demographic record).
/// </summary>
public sealed class PatientToDemographicsMapper : FhirToOpenEhrMapperBase<OpenEhrComposition>
{
    /// <summary>Creates the mapper with a FHIR <c>Patient</c> payload validator.</summary>
    public PatientToDemographicsMapper()
        : base(new FhirResourceValidator("Patient"))
    {
    }

    /// <inheritdoc />
    public override string SupportedFhirResourceType => "Patient";

    /// <inheritdoc />
    protected override MappingResult<OpenEhrComposition> MapCore(
        string fhirJson,
        IReadOnlyList<ValidationIssue> carriedIssues)
    {
        var patient = FhirSerialization.Parser.Parse<Hl7.Fhir.Model.Patient>(fhirJson);

        var primaryName = patient.Name.FirstOrDefault();
        var primaryAddress = patient.Address.FirstOrDefault();

        var demographics = new OpenEhrDemographics
        {
            FamilyName = primaryName?.Family,
            GivenName = primaryName?.Given?.FirstOrDefault(),
            Gender = GenderMap.ToOpenEhr(patient.Gender),
            BirthDate = patient.BirthDate,
            Identifiers = patient.Identifier
                .Where(id => !string.IsNullOrWhiteSpace(id.Value))
                .Select(id => new OpenEhrIdentifier
                {
                    Id = id.Value,
                    Issuer = id.System,
                    Type = id.Type?.Text ?? id.Type?.Coding?.FirstOrDefault()?.Code
                })
                .ToList(),
            Address = primaryAddress is null
                ? null
                : new OpenEhrAddress
                {
                    Line = primaryAddress.Line?.FirstOrDefault(),
                    City = primaryAddress.City,
                    PostalCode = primaryAddress.PostalCode,
                    Country = primaryAddress.Country
                }
        };

        var composition = new OpenEhrComposition
        {
            ArchetypeNodeId = "openEHR-EHR-COMPOSITION.demographics.v1",
            Name = "Demographics",
            Category = "persistent",
            StartTime = DateTimeOffset.UtcNow,
            EhrStatus = new OpenEhrEhrStatus
            {
                SubjectId = string.IsNullOrWhiteSpace(patient.Id)
                    ? Guid.NewGuid().ToString()
                    : patient.Id,
                SubjectNamespace = "DEMOGRAPHIC",
                IsQueryable = true,
                IsModifiable = true
            },
            Demographics = demographics
        };

        return MappingResult<OpenEhrComposition>.Ok(composition, carriedIssues);
    }
}
