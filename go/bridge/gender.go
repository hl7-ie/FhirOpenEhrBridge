package bridge

import "strings"

// GenderToOpenEhr maps a FHIR administrative-gender code to the openEHR local code.
func GenderToOpenEhr(fhirGender string) string {
	switch strings.ToLower(strings.TrimSpace(fhirGender)) {
	case "male":
		return "male"
	case "female":
		return "female"
	case "other":
		return "intersex"
	default:
		return "unknown"
	}
}

// GenderToFhir maps an openEHR gender code to a FHIR administrative-gender code.
// An empty input returns "" (meaning "do not set gender").
func GenderToFhir(openEhrGender string) string {
	switch strings.ToLower(strings.TrimSpace(openEhrGender)) {
	case "male":
		return "male"
	case "female":
		return "female"
	case "intersex":
		return "other"
	case "unknown":
		return "unknown"
	case "":
		return ""
	default:
		return "unknown"
	}
}
