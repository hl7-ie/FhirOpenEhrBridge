//! Cucumber BDD suite (run via `cargo test --test bdd`, harness = false).

use cucumber::{given, then, when, World};
use fhir_openehr_bridge::models::*;
use fhir_openehr_bridge::TranslationService;
use serde_json::Value;

#[derive(cucumber::World, Debug, Default)]
struct BridgeWorld {
    fhir_json: String,
    comp: Option<OpenEhrComposition>,
    fhir_outcome: Option<Outcome<OpenEhrComposition>>,
    bundle_outcome: Option<Outcome<Value>>,
}

impl BridgeWorld {
    fn succeeded(&self) -> bool {
        self.fhir_outcome
            .as_ref()
            .map(|o| o.success)
            .or_else(|| self.bundle_outcome.as_ref().map(|o| o.success))
            .expect("a translation was run")
    }
}

#[given("a valid FHIR Patient JSON")]
fn valid_patient(w: &mut BridgeWorld) {
    w.fhir_json = r#"{"resourceType":"Patient","id":"bdd-1","name":[{"family":"Smith","given":["John"]}],"gender":"male","birthDate":"1980-05-15"}"#.into();
}

#[given("a FHIR Observation JSON")]
fn observation(w: &mut BridgeWorld) {
    w.fhir_json = r#"{"resourceType":"Observation","status":"final"}"#.into();
}

#[given("a valid openEHR demographics composition")]
fn valid_comp(w: &mut BridgeWorld) {
    w.comp = Some(OpenEhrComposition {
        archetype_node_id: "openEHR-EHR-COMPOSITION.demographics.v1".into(),
        name: "Demographics".into(),
        language: "en".into(),
        territory: "GB".into(),
        category: "persistent".into(),
        start_time: None,
        ehr_status: OpenEhrEhrStatus { subject_id: Some("bdd-2".into()), ..Default::default() },
        demographics: Some(OpenEhrDemographics {
            family_name: Some("Smith".into()),
            given_name: Some("John".into()),
            gender: Some("male".into()),
            birth_date: None,
            identifiers: vec![],
            address: None,
        }),
    });
}

#[given("an openEHR composition without demographics")]
fn comp_without_demographics(w: &mut BridgeWorld) {
    w.comp = Some(OpenEhrComposition {
        archetype_node_id: "openEHR-EHR-COMPOSITION.demographics.v1".into(),
        name: String::new(),
        language: String::new(),
        territory: String::new(),
        category: String::new(),
        start_time: None,
        ehr_status: OpenEhrEhrStatus { subject_id: Some("bdd-3".into()), ..Default::default() },
        demographics: None,
    });
}

#[when("I translate it to openEHR")]
fn translate_to_openehr(w: &mut BridgeWorld) {
    w.fhir_outcome = Some(TranslationService::new().fhir_to_openehr(&w.fhir_json));
}

#[when("I translate it to FHIR")]
fn translate_to_fhir(w: &mut BridgeWorld) {
    let comp = w.comp.clone().expect("composition set");
    w.bundle_outcome = Some(TranslationService::new().openehr_to_fhir(&comp));
}

#[then("the translation succeeds")]
fn succeeds(w: &mut BridgeWorld) {
    assert!(w.succeeded(), "expected translation to succeed");
}

#[then("the translation fails")]
fn fails(w: &mut BridgeWorld) {
    assert!(!w.succeeded(), "expected translation to fail");
}

#[then(regex = r#"^the openEHR demographics family name is "([^"]*)"$"#)]
fn family_name(w: &mut BridgeWorld, expected: String) {
    let actual = w
        .fhir_outcome
        .as_ref()
        .and_then(|o| o.value.as_ref())
        .and_then(|c| c.demographics.as_ref())
        .and_then(|d| d.family_name.clone())
        .unwrap_or_default();
    assert_eq!(actual, expected);
}

#[then("the result is a FHIR Bundle containing a Patient")]
fn bundle_with_patient(w: &mut BridgeWorld) {
    let value = w.bundle_outcome.as_ref().and_then(|o| o.value.as_ref()).expect("bundle");
    assert_eq!(value["resourceType"], "Bundle");
    assert_eq!(value["entry"][0]["resource"]["resourceType"], "Patient");
}

#[tokio::main]
async fn main() {
    BridgeWorld::cucumber().run_and_exit("features").await;
}
