//! Inbound payload validation.

use crate::models::*;
use serde_json::Value;

/// Parses and validates inbound FHIR Patient JSON, returning issues and (on
/// success) the parsed JSON value.
pub fn validate_fhir_patient_json(json: &str) -> (Vec<ValidationIssue>, Option<Value>) {
    let mut issues = Vec::new();

    if json.trim().is_empty() {
        issues.push(ValidationIssue::error("FHIR payload is empty.", None));
        return (issues, None);
    }

    let parsed: Value = match serde_json::from_str(json) {
        Ok(v) => v,
        Err(e) => {
            issues.push(ValidationIssue::error(
                format!("Payload is not valid FHIR JSON: {e}"),
                None,
            ));
            return (issues, None);
        }
    };

    let resource_type = parsed.get("resourceType").and_then(|v| v.as_str());
    if resource_type != Some("Patient") {
        issues.push(ValidationIssue::error(
            format!(
                "Expected a FHIR 'Patient' resource but received '{}'.",
                resource_type.unwrap_or("")
            ),
            Some("resourceType"),
        ));
        return (issues, None);
    }

    (issues, Some(parsed))
}

/// Validates an openEHR composition before mapping.
pub fn validate_composition(c: &OpenEhrComposition) -> Vec<ValidationIssue> {
    let mut issues = Vec::new();

    if c.archetype_node_id.trim().is_empty() {
        issues.push(ValidationIssue::error(
            "Composition is missing 'archetypeNodeId'.",
            Some("archetypeNodeId"),
        ));
    }

    match &c.demographics {
        None => issues.push(ValidationIssue::error(
            "Composition does not contain a demographics payload.",
            Some("demographics"),
        )),
        Some(d) => {
            let no_family = d.family_name.as_deref().unwrap_or("").is_empty();
            let no_given = d.given_name.as_deref().unwrap_or("").is_empty();
            if no_family && no_given {
                issues.push(ValidationIssue::warning(
                    "Demographics contain neither a family name nor a given name.",
                    Some("demographics.familyName"),
                ));
            }
        }
    }

    if c.ehr_status.subject_id.as_deref().unwrap_or("").trim().is_empty() {
        issues.push(ValidationIssue::warning(
            "EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.",
            Some("ehrStatus.subjectId"),
        ));
    }

    issues
}
