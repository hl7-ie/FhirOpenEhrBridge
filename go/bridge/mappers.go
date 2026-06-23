package bridge

import (
	"crypto/rand"
	"encoding/json"
	"fmt"
	"strings"
	"time"
)

const demographicsArchetype = "openEHR-EHR-COMPOSITION.demographics.v1"

func newUUID() string {
	b := make([]byte, 16)
	_, _ = rand.Read(b)
	b[6] = (b[6] & 0x0f) | 0x40
	b[8] = (b[8] & 0x3f) | 0x80
	return fmt.Sprintf("%x-%x-%x-%x-%x", b[0:4], b[4:6], b[6:8], b[8:10], b[10:16])
}

// FhirPatientToComposition maps FHIR Patient JSON to an openEHR composition.
func FhirPatientToComposition(fhirJSON string) Result[*OpenEhrComposition] {
	issues, p := validateFhirPatientJSON(fhirJSON)
	if hasError(issues) || p == nil {
		return Result[*OpenEhrComposition]{Success: false, Issues: issues}
	}

	demo := &OpenEhrDemographics{
		Gender:      GenderToOpenEhr(p.Gender),
		BirthDate:   p.BirthDate,
		Identifiers: []OpenEhrIdentifier{},
	}
	if len(p.Name) > 0 {
		demo.FamilyName = p.Name[0].Family
		if len(p.Name[0].Given) > 0 {
			demo.GivenName = p.Name[0].Given[0]
		}
	}
	for _, id := range p.Identifier {
		if strings.TrimSpace(id.Value) == "" {
			continue
		}
		t := ""
		if id.Type != nil {
			if id.Type.Text != "" {
				t = id.Type.Text
			} else if len(id.Type.Coding) > 0 {
				t = id.Type.Coding[0].Code
			}
		}
		demo.Identifiers = append(demo.Identifiers, OpenEhrIdentifier{ID: id.Value, Issuer: id.System, Type: t})
	}
	if len(p.Address) > 0 {
		a := p.Address[0]
		line := ""
		if len(a.Line) > 0 {
			line = a.Line[0]
		}
		demo.Address = &OpenEhrAddress{Line: line, City: a.City, PostalCode: a.PostalCode, Country: a.Country}
	}

	subject := p.ID
	if strings.TrimSpace(subject) == "" {
		subject = newUUID()
	}

	comp := &OpenEhrComposition{
		ArchetypeNodeID: demographicsArchetype,
		Name:            "Demographics",
		Language:        "en",
		Territory:       "GB",
		Category:        "persistent",
		StartTime:       time.Now().UTC().Format(time.RFC3339),
		EhrStatus:       OpenEhrEhrStatus{SubjectID: subject, SubjectNamespace: "DEMOGRAPHIC", IsQueryable: true, IsModifiable: true},
		Demographics:    demo,
	}
	return Result[*OpenEhrComposition]{Success: true, Value: comp, Issues: issues}
}

// CompositionToFhirBundle maps an openEHR composition to a FHIR Bundle JSON string.
func CompositionToFhirBundle(c *OpenEhrComposition) Result[string] {
	issues := ValidateComposition(c)
	if hasError(issues) {
		return Result[string]{Success: false, Issues: issues}
	}

	d := c.Demographics
	patientID := c.EhrStatus.SubjectID
	if strings.TrimSpace(patientID) == "" {
		patientID = newUUID()
	}

	patient := map[string]any{"resourceType": "Patient", "id": patientID}

	if d.FamilyName != "" || d.GivenName != "" {
		name := map[string]any{"use": "official"}
		if d.FamilyName != "" {
			name["family"] = d.FamilyName
		}
		if d.GivenName != "" {
			name["given"] = []string{d.GivenName}
		}
		patient["name"] = []any{name}
	}
	if g := GenderToFhir(d.Gender); g != "" {
		patient["gender"] = g
	}
	if d.BirthDate != "" {
		patient["birthDate"] = d.BirthDate
	}
	var identifiers []any
	for _, id := range d.Identifiers {
		if strings.TrimSpace(id.ID) == "" {
			continue
		}
		fi := map[string]any{"system": id.Issuer, "value": id.ID}
		if id.Type != "" {
			fi["type"] = map[string]any{"text": id.Type}
		}
		identifiers = append(identifiers, fi)
	}
	if len(identifiers) > 0 {
		patient["identifier"] = identifiers
	}
	if d.Address != nil {
		addr := map[string]any{}
		if d.Address.Line != "" {
			addr["line"] = []string{d.Address.Line}
		}
		if d.Address.City != "" {
			addr["city"] = d.Address.City
		}
		if d.Address.PostalCode != "" {
			addr["postalCode"] = d.Address.PostalCode
		}
		if d.Address.Country != "" {
			addr["country"] = d.Address.Country
		}
		patient["address"] = []any{addr}
	}

	timestamp := c.StartTime
	if timestamp == "" {
		timestamp = time.Now().UTC().Format(time.RFC3339)
	}
	bundle := map[string]any{
		"resourceType": "Bundle",
		"id":           newUUID(),
		"type":         "collection",
		"timestamp":    timestamp,
		"entry":        []any{map[string]any{"fullUrl": "urn:uuid:" + patientID, "resource": patient}},
	}

	out, err := json.Marshal(bundle)
	if err != nil {
		return Result[string]{Success: false, Issues: []ValidationIssue{{SeverityError, "Failed to serialise FHIR Bundle: " + err.Error(), ""}}}
	}
	return Result[string]{Success: true, Value: string(out), Issues: issues}
}
