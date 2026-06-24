// Package bridge implements bidirectional FHIR <-> openEHR demographics
// translation. It mirrors the .NET reference implementation; see
// docs/POLYGLOT.md for the shared, language-neutral contract.
package bridge

// OpenEhrIdentifier is an openEHR DV_IDENTIFIER (a business identifier).
type OpenEhrIdentifier struct {
	ID     string `json:"id,omitempty"`
	Issuer string `json:"issuer,omitempty"`
	Type   string `json:"type,omitempty"`
}

// OpenEhrAddress is a simplified openEHR address cluster.
type OpenEhrAddress struct {
	Line       string `json:"line,omitempty"`
	City       string `json:"city,omitempty"`
	PostalCode string `json:"postalCode,omitempty"`
	Country    string `json:"country,omitempty"`
}

// OpenEhrDemographics carries the demographic content of a composition.
type OpenEhrDemographics struct {
	FamilyName  string              `json:"familyName,omitempty"`
	GivenName   string              `json:"givenName,omitempty"`
	Gender      string              `json:"gender,omitempty"`
	BirthDate   string              `json:"birthDate,omitempty"`
	Identifiers []OpenEhrIdentifier `json:"identifiers"`
	Address     *OpenEhrAddress     `json:"address,omitempty"`
}

// OpenEhrEhrStatus links an EHR to its demographic subject.
type OpenEhrEhrStatus struct {
	SubjectID        string `json:"subjectId,omitempty"`
	SubjectNamespace string `json:"subjectNamespace,omitempty"`
	IsQueryable      bool   `json:"isQueryable"`
	IsModifiable     bool   `json:"isModifiable"`
}

// OpenEhrComposition is a simplified openEHR demographics COMPOSITION.
type OpenEhrComposition struct {
	ArchetypeNodeID string               `json:"archetypeNodeId"`
	Name            string               `json:"name"`
	Language        string               `json:"language"`
	Territory       string               `json:"territory"`
	Category        string               `json:"category"`
	StartTime       string               `json:"startTime,omitempty"`
	EhrStatus       OpenEhrEhrStatus     `json:"ehrStatus"`
	Demographics    *OpenEhrDemographics `json:"demographics"`
}

// Severity classifies a ValidationIssue.
type Severity string

const (
	SeverityError       Severity = "error"
	SeverityWarning     Severity = "warning"
	SeverityInformation Severity = "information"
)

// ValidationIssue describes a single problem found while validating a payload.
type ValidationIssue struct {
	Severity Severity `json:"severity"`
	Message  string   `json:"message"`
	Location string   `json:"location,omitempty"`
}

// Result is the outcome of a translation in either direction.
type Result[T any] struct {
	Success bool              `json:"success"`
	Value   T                 `json:"value"`
	Issues  []ValidationIssue `json:"issues"`
}

func hasError(issues []ValidationIssue) bool {
	for _, i := range issues {
		if i.Severity == SeverityError {
			return true
		}
	}
	return false
}

// fhirPatient is the subset of a FHIR Patient used by the demographics mapping.
type fhirPatient struct {
	ResourceType string `json:"resourceType"`
	ID           string `json:"id"`
	Identifier   []struct {
		System string `json:"system"`
		Value  string `json:"value"`
		Type   *struct {
			Text   string `json:"text"`
			Coding []struct {
				Code string `json:"code"`
			} `json:"coding"`
		} `json:"type"`
	} `json:"identifier"`
	Name []struct {
		Family string   `json:"family"`
		Given  []string `json:"given"`
	} `json:"name"`
	Gender    string `json:"gender"`
	BirthDate string `json:"birthDate"`
	Address   []struct {
		Line       []string `json:"line"`
		City       string   `json:"city"`
		PostalCode string   `json:"postalCode"`
		Country    string   `json:"country"`
	} `json:"address"`
}
