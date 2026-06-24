package bridge

import (
	"encoding/json"
	"strings"
)

// validateFhirPatientJSON parses and validates inbound FHIR Patient JSON.
func validateFhirPatientJSON(jsonStr string) ([]ValidationIssue, *fhirPatient) {
	issues := []ValidationIssue{}

	if strings.TrimSpace(jsonStr) == "" {
		return append(issues, ValidationIssue{SeverityError, "FHIR payload is empty.", ""}), nil
	}

	var p fhirPatient
	if err := json.Unmarshal([]byte(jsonStr), &p); err != nil {
		return append(issues, ValidationIssue{SeverityError, "Payload is not valid FHIR JSON: " + err.Error(), ""}), nil
	}

	if p.ResourceType != "Patient" {
		return append(issues, ValidationIssue{
			SeverityError,
			"Expected a FHIR 'Patient' resource but received '" + p.ResourceType + "'.",
			"resourceType",
		}), nil
	}

	return issues, &p
}

// ValidateComposition validates an openEHR composition before mapping.
func ValidateComposition(c *OpenEhrComposition) []ValidationIssue {
	issues := []ValidationIssue{}
	if c == nil {
		return append(issues, ValidationIssue{SeverityError, "openEHR composition is null.", ""})
	}
	if strings.TrimSpace(c.ArchetypeNodeID) == "" {
		issues = append(issues, ValidationIssue{SeverityError, "Composition is missing 'archetypeNodeId'.", "archetypeNodeId"})
	}
	if c.Demographics == nil {
		issues = append(issues, ValidationIssue{SeverityError, "Composition does not contain a demographics payload.", "demographics"})
	} else if c.Demographics.FamilyName == "" && c.Demographics.GivenName == "" {
		issues = append(issues, ValidationIssue{SeverityWarning, "Demographics contain neither a family name nor a given name.", "demographics.familyName"})
	}
	if strings.TrimSpace(c.EhrStatus.SubjectID) == "" {
		issues = append(issues, ValidationIssue{SeverityWarning, "EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.", "ehrStatus.subjectId"})
	}
	return issues
}
