package bridge

import (
	"encoding/json"
	"testing"
)

const patientJSON = `{
  "resourceType": "Patient",
  "id": "example-123",
  "identifier": [{ "system": "https://fhir.nhs.uk/Id/nhs-number", "value": "9876543210", "type": { "text": "NHS" } }],
  "name": [{ "use": "official", "family": "Smith", "given": ["John"] }],
  "gender": "male",
  "birthDate": "1980-05-15",
  "address": [{ "line": ["10 Downing Street"], "city": "London", "postalCode": "SW1A 2AA", "country": "GB" }]
}`

func sampleComposition() *OpenEhrComposition {
	return &OpenEhrComposition{
		ArchetypeNodeID: demographicsArchetype,
		EhrStatus:       OpenEhrEhrStatus{SubjectID: "example-123"},
		Demographics: &OpenEhrDemographics{
			FamilyName: "Smith", GivenName: "John", Gender: "male", BirthDate: "1980-05-15",
			Identifiers: []OpenEhrIdentifier{{ID: "9876543210", Issuer: "https://fhir.nhs.uk/Id/nhs-number", Type: "NHS"}},
			Address:     &OpenEhrAddress{Line: "10 Downing Street", City: "London", PostalCode: "SW1A 2AA", Country: "GB"},
		},
	}
}

func TestFhirPatientToComposition(t *testing.T) {
	res := FhirPatientToComposition(patientJSON)
	if !res.Success {
		t.Fatalf("expected success, issues=%v", res.Issues)
	}
	d := res.Value.Demographics
	if d.FamilyName != "Smith" || d.GivenName != "John" || d.Gender != "male" || d.BirthDate != "1980-05-15" {
		t.Fatalf("unexpected demographics: %+v", d)
	}
	if len(d.Identifiers) != 1 || d.Identifiers[0].ID != "9876543210" {
		t.Fatalf("unexpected identifiers: %+v", d.Identifiers)
	}
	if res.Value.EhrStatus.SubjectID != "example-123" {
		t.Fatalf("unexpected subject id: %s", res.Value.EhrStatus.SubjectID)
	}
}

func TestFhirRejectsNonPatient(t *testing.T) {
	res := FhirPatientToComposition(`{"resourceType":"Observation","status":"final"}`)
	if res.Success {
		t.Fatal("expected failure for Observation")
	}
	if !hasError(res.Issues) {
		t.Fatal("expected an error-severity issue")
	}
}

func TestFhirRejectsInvalidJSON(t *testing.T) {
	for _, bad := range []string{"", "   ", "{ not json"} {
		if FhirPatientToComposition(bad).Success {
			t.Fatalf("expected failure for %q", bad)
		}
	}
}

func TestCompositionToFhirBundle(t *testing.T) {
	res := CompositionToFhirBundle(sampleComposition())
	if !res.Success {
		t.Fatalf("expected success, issues=%v", res.Issues)
	}
	var bundle map[string]any
	if err := json.Unmarshal([]byte(res.Value), &bundle); err != nil {
		t.Fatal(err)
	}
	if bundle["resourceType"] != "Bundle" || bundle["type"] != "collection" {
		t.Fatalf("unexpected bundle: %v", bundle)
	}
	entry := bundle["entry"].([]any)[0].(map[string]any)
	patient := entry["resource"].(map[string]any)
	if patient["id"] != "example-123" || patient["gender"] != "male" {
		t.Fatalf("unexpected patient: %v", patient)
	}
}

func TestCompositionRejectsMissingDemographics(t *testing.T) {
	c := sampleComposition()
	c.Demographics = nil
	if CompositionToFhirBundle(c).Success {
		t.Fatal("expected failure when demographics missing")
	}
}

func TestRoundTrip(t *testing.T) {
	comp := FhirPatientToComposition(patientJSON)
	fhir := CompositionToFhirBundle(comp.Value)
	var bundle map[string]any
	_ = json.Unmarshal([]byte(fhir.Value), &bundle)
	patient := bundle["entry"].([]any)[0].(map[string]any)["resource"].(map[string]any)
	if patient["id"] != "example-123" {
		t.Fatalf("round-trip lost id: %v", patient["id"])
	}
	idents := patient["identifier"].([]any)
	first := idents[0].(map[string]any)
	if first["value"] != "9876543210" {
		t.Fatalf("round-trip lost identifier: %v", first)
	}
}
