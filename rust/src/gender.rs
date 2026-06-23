//! Gender mapping between FHIR administrative-gender codes and the openEHR
//! local value set.

pub fn gender_to_openehr(fhir_gender: Option<&str>) -> String {
    match fhir_gender.unwrap_or("").trim().to_lowercase().as_str() {
        "male" => "male",
        "female" => "female",
        "other" => "intersex",
        _ => "unknown",
    }
    .to_string()
}

/// Returns `None` when the input is empty (meaning "do not set gender").
pub fn gender_to_fhir(openehr_gender: Option<&str>) -> Option<String> {
    match openehr_gender.unwrap_or("").trim().to_lowercase().as_str() {
        "male" => Some("male".to_string()),
        "female" => Some("female".to_string()),
        "intersex" => Some("other".to_string()),
        "unknown" => Some("unknown".to_string()),
        "" => None,
        _ => Some("unknown".to_string()),
    }
}
