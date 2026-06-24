//! Bidirectional mappers.

use crate::gender::{gender_to_fhir, gender_to_openehr};
use crate::models::*;
use crate::validation::{validate_composition, validate_fhir_patient_json};
use serde_json::{json, Map, Value};
use uuid::Uuid;

const ARCHETYPE: &str = "openEHR-EHR-COMPOSITION.demographics.v1";

/// FHIR Patient JSON -> openEHR demographics composition.
pub fn fhir_patient_to_composition(fhir_json: &str) -> Outcome<OpenEhrComposition> {
    let (issues, parsed) = validate_fhir_patient_json(fhir_json);
    if has_error(&issues) {
        return Outcome::fail(issues);
    }
    let p = parsed.expect("validated payload");

    let name0 = p.get("name").and_then(|n| n.get(0));
    let family = name0
        .and_then(|n| n.get("family"))
        .and_then(|v| v.as_str())
        .map(String::from);
    let given = name0
        .and_then(|n| n.get("given"))
        .and_then(|g| g.get(0))
        .and_then(|v| v.as_str())
        .map(String::from);

    let mut identifiers = Vec::new();
    if let Some(arr) = p.get("identifier").and_then(|v| v.as_array()) {
        for id in arr {
            let value = id.get("value").and_then(|v| v.as_str());
            if value.map(|s| s.trim().is_empty()).unwrap_or(true) {
                continue;
            }
            let type_ = id.get("type").and_then(|t| {
                t.get("text")
                    .and_then(|v| v.as_str())
                    .or_else(|| {
                        t.get("coding")
                            .and_then(|c| c.get(0))
                            .and_then(|c| c.get("code"))
                            .and_then(|v| v.as_str())
                    })
                    .map(String::from)
            });
            identifiers.push(OpenEhrIdentifier {
                id: value.map(String::from),
                issuer: id.get("system").and_then(|v| v.as_str()).map(String::from),
                type_,
            });
        }
    }

    let address = p.get("address").and_then(|a| a.get(0)).map(|a| OpenEhrAddress {
        line: a
            .get("line")
            .and_then(|l| l.get(0))
            .and_then(|v| v.as_str())
            .map(String::from),
        city: a.get("city").and_then(|v| v.as_str()).map(String::from),
        postal_code: a.get("postalCode").and_then(|v| v.as_str()).map(String::from),
        country: a.get("country").and_then(|v| v.as_str()).map(String::from),
    });

    let subject = p
        .get("id")
        .and_then(|v| v.as_str())
        .filter(|s| !s.trim().is_empty())
        .map(String::from)
        .unwrap_or_else(|| Uuid::new_v4().to_string());

    let comp = OpenEhrComposition {
        archetype_node_id: ARCHETYPE.to_string(),
        name: "Demographics".to_string(),
        language: "en".to_string(),
        territory: "GB".to_string(),
        category: "persistent".to_string(),
        start_time: None,
        ehr_status: OpenEhrEhrStatus {
            subject_id: Some(subject),
            ..Default::default()
        },
        demographics: Some(OpenEhrDemographics {
            family_name: family,
            given_name: given,
            gender: Some(gender_to_openehr(p.get("gender").and_then(|v| v.as_str()))),
            birth_date: p.get("birthDate").and_then(|v| v.as_str()).map(String::from),
            identifiers,
            address,
        }),
    };

    Outcome::ok(comp, issues)
}

/// openEHR demographics composition -> FHIR Bundle (as a JSON value).
pub fn composition_to_fhir_bundle(c: &OpenEhrComposition) -> Outcome<Value> {
    let issues = validate_composition(c);
    if has_error(&issues) {
        return Outcome::fail(issues);
    }
    let d = c.demographics.as_ref().expect("validated demographics");

    let patient_id = c
        .ehr_status
        .subject_id
        .clone()
        .filter(|s| !s.trim().is_empty())
        .unwrap_or_else(|| Uuid::new_v4().to_string());

    let mut patient = Map::new();
    patient.insert("resourceType".into(), json!("Patient"));
    patient.insert("id".into(), json!(patient_id));

    if d.family_name.is_some() || d.given_name.is_some() {
        let mut name = Map::new();
        name.insert("use".into(), json!("official"));
        if let Some(f) = &d.family_name {
            name.insert("family".into(), json!(f));
        }
        if let Some(g) = &d.given_name {
            name.insert("given".into(), json!([g]));
        }
        patient.insert("name".into(), json!([Value::Object(name)]));
    }

    if let Some(g) = gender_to_fhir(d.gender.as_deref()) {
        patient.insert("gender".into(), json!(g));
    }
    if let Some(bd) = &d.birth_date {
        patient.insert("birthDate".into(), json!(bd));
    }

    let idents: Vec<Value> = d
        .identifiers
        .iter()
        .filter(|i| i.id.as_deref().map(|s| !s.trim().is_empty()).unwrap_or(false))
        .map(|i| {
            let mut m = Map::new();
            if let Some(s) = &i.issuer {
                m.insert("system".into(), json!(s));
            }
            m.insert("value".into(), json!(i.id));
            if let Some(t) = &i.type_ {
                m.insert("type".into(), json!({ "text": t }));
            }
            Value::Object(m)
        })
        .collect();
    if !idents.is_empty() {
        patient.insert("identifier".into(), json!(idents));
    }

    if let Some(a) = &d.address {
        let mut m = Map::new();
        if let Some(l) = &a.line {
            m.insert("line".into(), json!([l]));
        }
        if let Some(city) = &a.city {
            m.insert("city".into(), json!(city));
        }
        if let Some(pc) = &a.postal_code {
            m.insert("postalCode".into(), json!(pc));
        }
        if let Some(co) = &a.country {
            m.insert("country".into(), json!(co));
        }
        patient.insert("address".into(), json!([Value::Object(m)]));
    }

    let mut bundle = Map::new();
    bundle.insert("resourceType".into(), json!("Bundle"));
    bundle.insert("id".into(), json!(Uuid::new_v4().to_string()));
    bundle.insert("type".into(), json!("collection"));
    if let Some(ts) = &c.start_time {
        bundle.insert("timestamp".into(), json!(ts));
    }
    bundle.insert(
        "entry".into(),
        json!([{ "fullUrl": format!("urn:uuid:{patient_id}"), "resource": Value::Object(patient) }]),
    );

    Outcome::ok(Value::Object(bundle), issues)
}

#[cfg(test)]
mod tests {
    use super::*;

    const PATIENT_JSON: &str = r#"{
        "resourceType": "Patient", "id": "example-123",
        "identifier": [{ "system": "https://fhir.nhs.uk/Id/nhs-number", "value": "9876543210", "type": { "text": "NHS" } }],
        "name": [{ "family": "Smith", "given": ["John"] }],
        "gender": "male", "birthDate": "1980-05-15",
        "address": [{ "line": ["10 Downing Street"], "city": "London", "postalCode": "SW1A 2AA", "country": "GB" }]
    }"#;

    fn sample_composition() -> OpenEhrComposition {
        OpenEhrComposition {
            archetype_node_id: ARCHETYPE.to_string(),
            name: "Demographics".into(),
            language: "en".into(),
            territory: "GB".into(),
            category: "persistent".into(),
            start_time: None,
            ehr_status: OpenEhrEhrStatus { subject_id: Some("example-123".into()), ..Default::default() },
            demographics: Some(OpenEhrDemographics {
                family_name: Some("Smith".into()),
                given_name: Some("John".into()),
                gender: Some("male".into()),
                birth_date: Some("1980-05-15".into()),
                identifiers: vec![OpenEhrIdentifier { id: Some("9876543210".into()), issuer: Some("https://fhir.nhs.uk/Id/nhs-number".into()), type_: Some("NHS".into()) }],
                address: Some(OpenEhrAddress { line: Some("10 Downing Street".into()), city: Some("London".into()), postal_code: Some("SW1A 2AA".into()), country: Some("GB".into()) }),
            }),
        }
    }

    #[test]
    fn maps_valid_patient() {
        let out = fhir_patient_to_composition(PATIENT_JSON);
        assert!(out.success);
        let d = out.value.unwrap().demographics.unwrap();
        assert_eq!(d.family_name.as_deref(), Some("Smith"));
        assert_eq!(d.given_name.as_deref(), Some("John"));
        assert_eq!(d.gender.as_deref(), Some("male"));
        assert_eq!(d.identifiers.len(), 1);
        assert_eq!(d.identifiers[0].id.as_deref(), Some("9876543210"));
    }

    #[test]
    fn rejects_non_patient() {
        let out = fhir_patient_to_composition(r#"{"resourceType":"Observation","status":"final"}"#);
        assert!(!out.success);
        assert!(has_error(&out.issues));
    }

    #[test]
    fn rejects_invalid_json() {
        for bad in ["", "   ", "{ not json"] {
            assert!(!fhir_patient_to_composition(bad).success);
        }
    }

    #[test]
    fn maps_composition_to_bundle() {
        let out = composition_to_fhir_bundle(&sample_composition());
        assert!(out.success);
        let bundle = out.value.unwrap();
        assert_eq!(bundle["resourceType"], "Bundle");
        assert_eq!(bundle["type"], "collection");
        let patient = &bundle["entry"][0]["resource"];
        assert_eq!(patient["id"], "example-123");
        assert_eq!(patient["gender"], "male");
        assert_eq!(patient["identifier"][0]["value"], "9876543210");
    }

    #[test]
    fn rejects_missing_demographics() {
        let mut c = sample_composition();
        c.demographics = None;
        assert!(!composition_to_fhir_bundle(&c).success);
    }

    #[test]
    fn round_trip_preserves_core() {
        let comp = fhir_patient_to_composition(PATIENT_JSON).value.unwrap();
        let bundle = composition_to_fhir_bundle(&comp).value.unwrap();
        let patient = &bundle["entry"][0]["resource"];
        assert_eq!(patient["id"], "example-123");
        assert_eq!(patient["name"][0]["family"], "Smith");
        assert_eq!(patient["identifier"][0]["value"], "9876543210");
    }
}
