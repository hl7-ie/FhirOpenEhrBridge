package features

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"testing"

	"github.com/cucumber/godog"
	"github.com/hl7-ie/FhirOpenEhrBridge/go/bridge"
)

type world struct {
	svc        *bridge.TranslationService
	fhirJSON   string
	comp       *bridge.OpenEhrComposition
	fhirRes    bridge.Result[*bridge.OpenEhrComposition]
	bundleRes  bridge.Result[string]
	ranToFhir  bool
}

func (w *world) reset(*godog.Scenario) {
	w.svc = bridge.NewTranslationService()
	w.fhirJSON = ""
	w.comp = nil
	w.ranToFhir = false
}

func (w *world) aValidFhirPatientJSON() {
	w.fhirJSON = `{"resourceType":"Patient","id":"bdd-1","name":[{"family":"Smith","given":["John"]}],"gender":"male","birthDate":"1980-05-15"}`
}

func (w *world) aFhirObservationJSON() {
	w.fhirJSON = `{"resourceType":"Observation","status":"final"}`
}

func (w *world) aValidComposition() {
	w.comp = &bridge.OpenEhrComposition{
		ArchetypeNodeID: "openEHR-EHR-COMPOSITION.demographics.v1",
		EhrStatus:       bridge.OpenEhrEhrStatus{SubjectID: "bdd-2"},
		Demographics:    &bridge.OpenEhrDemographics{FamilyName: "Smith", GivenName: "John", Gender: "male"},
	}
}

func (w *world) aCompositionWithoutDemographics() {
	w.comp = &bridge.OpenEhrComposition{
		ArchetypeNodeID: "openEHR-EHR-COMPOSITION.demographics.v1",
		EhrStatus:       bridge.OpenEhrEhrStatus{SubjectID: "bdd-3"},
		Demographics:    nil,
	}
}

func (w *world) translateToOpenEhr() {
	w.fhirRes = w.svc.FhirToOpenEhr(w.fhirJSON)
	w.ranToFhir = false
}

func (w *world) translateToFhir() {
	w.bundleRes = w.svc.OpenEhrToFhir(w.comp)
	w.ranToFhir = true
}

func (w *world) translationSucceeds() error {
	if w.ranToFhir {
		if !w.bundleRes.Success {
			return fmt.Errorf("expected success, issues=%v", w.bundleRes.Issues)
		}
		return nil
	}
	if !w.fhirRes.Success {
		return fmt.Errorf("expected success, issues=%v", w.fhirRes.Issues)
	}
	return nil
}

func (w *world) translationFails() error {
	ok := w.fhirRes.Success
	if w.ranToFhir {
		ok = w.bundleRes.Success
	}
	if ok {
		return fmt.Errorf("expected failure but translation succeeded")
	}
	return nil
}

func (w *world) familyNameIs(expected string) error {
	got := w.fhirRes.Value.Demographics.FamilyName
	if got != expected {
		return fmt.Errorf("expected family name %q, got %q", expected, got)
	}
	return nil
}

func (w *world) resultIsBundleWithPatient() error {
	var bundle map[string]any
	if err := json.Unmarshal([]byte(w.bundleRes.Value), &bundle); err != nil {
		return err
	}
	if bundle["resourceType"] != "Bundle" {
		return fmt.Errorf("expected Bundle, got %v", bundle["resourceType"])
	}
	entry := bundle["entry"].([]any)[0].(map[string]any)
	if entry["resource"].(map[string]any)["resourceType"] != "Patient" {
		return fmt.Errorf("expected Patient in entry")
	}
	return nil
}

func InitializeScenario(sc *godog.ScenarioContext) {
	w := &world{}
	sc.Before(func(ctx context.Context, scn *godog.Scenario) (context.Context, error) { w.reset(scn); return ctx, nil })
	sc.Step(`^a valid FHIR Patient JSON$`, w.aValidFhirPatientJSON)
	sc.Step(`^a FHIR Observation JSON$`, w.aFhirObservationJSON)
	sc.Step(`^a valid openEHR demographics composition$`, w.aValidComposition)
	sc.Step(`^an openEHR composition without demographics$`, w.aCompositionWithoutDemographics)
	sc.Step(`^I translate it to openEHR$`, w.translateToOpenEhr)
	sc.Step(`^I translate it to FHIR$`, w.translateToFhir)
	sc.Step(`^the translation succeeds$`, w.translationSucceeds)
	sc.Step(`^the translation fails$`, w.translationFails)
	sc.Step(`^the openEHR demographics family name is "([^"]*)"$`, w.familyNameIs)
	sc.Step(`^the result is a FHIR Bundle containing a Patient$`, w.resultIsBundleWithPatient)
}

func TestFeatures(t *testing.T) {
	suite := godog.TestSuite{
		ScenarioInitializer: InitializeScenario,
		Options: &godog.Options{
			Format:   "pretty",
			Paths:    []string{"."},
			TestingT: t,
			Output:   os.Stdout,
		},
	}
	if suite.Run() != 0 {
		t.Fatal("non-zero status: BDD feature tests failed")
	}
}
