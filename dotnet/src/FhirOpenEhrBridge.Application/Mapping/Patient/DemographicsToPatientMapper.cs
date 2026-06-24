using FhirOpenEhrBridge.Application.Fhir;
using FhirOpenEhrBridge.Application.Validation;
using FhirOpenEhrBridge.Domain.Mapping;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using FhirOpenEhrBridge.Domain.Validation;
using Hl7.Fhir.Model;

namespace FhirOpenEhrBridge.Application.Mapping.Patient;

/// <summary>
/// Proof-of-concept mapper translating an openEHR demographics
/// <see cref="OpenEhrComposition"/> into a FHIR <c>Bundle</c> (of type
/// <c>collection</c>) containing a single <c>Patient</c> resource. This is the
/// inverse of <see cref="PatientToDemographicsMapper"/>.
/// </summary>
public sealed class DemographicsToPatientMapper : OpenEhrToFhirMapperBase<OpenEhrComposition>
{
    /// <summary>Creates the mapper with an openEHR composition validator.</summary>
    public DemographicsToPatientMapper()
        : base(new OpenEhrCompositionValidator())
    {
    }

    /// <inheritdoc />
    public override string SupportedArchetypeId => "openEHR-EHR-COMPOSITION.demographics.v1";

    /// <inheritdoc />
    protected override MappingResult<string> MapCore(
        OpenEhrComposition openEhr,
        IReadOnlyList<ValidationIssue> carriedIssues)
    {
        var demographics = openEhr.Demographics;

        var patient = new Hl7.Fhir.Model.Patient
        {
            Id = string.IsNullOrWhiteSpace(openEhr.EhrStatus?.SubjectId)
                ? Guid.NewGuid().ToString()
                : openEhr.EhrStatus.SubjectId
        };

        if (!string.IsNullOrWhiteSpace(demographics.FamilyName) ||
            !string.IsNullOrWhiteSpace(demographics.GivenName))
        {
            var name = new HumanName { Use = HumanName.NameUse.Official };
            if (!string.IsNullOrWhiteSpace(demographics.FamilyName))
            {
                name.Family = demographics.FamilyName;
            }

            if (!string.IsNullOrWhiteSpace(demographics.GivenName))
            {
                name.Given = new[] { demographics.GivenName! };
            }

            patient.Name.Add(name);
        }

        var gender = GenderMap.ToFhir(demographics.Gender);
        if (gender.HasValue)
        {
            patient.Gender = gender;
        }

        if (!string.IsNullOrWhiteSpace(demographics.BirthDate))
        {
            patient.BirthDate = demographics.BirthDate;
        }

        foreach (var identifier in demographics.Identifiers.Where(i => !string.IsNullOrWhiteSpace(i.Id)))
        {
            var fhirIdentifier = new Identifier
            {
                System = identifier.Issuer,
                Value = identifier.Id
            };

            if (!string.IsNullOrWhiteSpace(identifier.Type))
            {
                fhirIdentifier.Type = new CodeableConcept { Text = identifier.Type };
            }

            patient.Identifier.Add(fhirIdentifier);
        }

        if (demographics.Address is { } address)
        {
            var fhirAddress = new Address
            {
                City = address.City,
                PostalCode = address.PostalCode,
                Country = address.Country
            };

            if (!string.IsNullOrWhiteSpace(address.Line))
            {
                fhirAddress.Line = new[] { address.Line! };
            }

            patient.Address.Add(fhirAddress);
        }

        var bundle = new Bundle
        {
            Id = Guid.NewGuid().ToString(),
            Type = Bundle.BundleType.Collection,
            Timestamp = openEhr.StartTime ?? DateTimeOffset.UtcNow
        };

        bundle.Entry.Add(new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{patient.Id}",
            Resource = patient
        });

        var json = FhirSerialization.Serializer.SerializeToString(bundle);
        return MappingResult<string>.Ok(json, carriedIssues);
    }
}
