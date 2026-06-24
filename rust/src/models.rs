//! Canonical, language-neutral models mirroring the .NET reference
//! implementation (see docs/POLYGLOT.md).

use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OpenEhrIdentifier {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub id: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub issuer: Option<String>,
    #[serde(rename = "type", skip_serializing_if = "Option::is_none")]
    pub type_: Option<String>,
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OpenEhrAddress {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub line: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub city: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub postal_code: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub country: Option<String>,
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OpenEhrDemographics {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub family_name: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub given_name: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub gender: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub birth_date: Option<String>,
    #[serde(default)]
    pub identifiers: Vec<OpenEhrIdentifier>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub address: Option<OpenEhrAddress>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OpenEhrEhrStatus {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub subject_id: Option<String>,
    #[serde(default = "default_namespace")]
    pub subject_namespace: String,
    #[serde(default = "default_true")]
    pub is_queryable: bool,
    #[serde(default = "default_true")]
    pub is_modifiable: bool,
}

fn default_namespace() -> String {
    "DEMOGRAPHIC".to_string()
}
fn default_true() -> bool {
    true
}

impl Default for OpenEhrEhrStatus {
    fn default() -> Self {
        Self {
            subject_id: None,
            subject_namespace: default_namespace(),
            is_queryable: true,
            is_modifiable: true,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct OpenEhrComposition {
    pub archetype_node_id: String,
    #[serde(default)]
    pub name: String,
    #[serde(default)]
    pub language: String,
    #[serde(default)]
    pub territory: String,
    #[serde(default)]
    pub category: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub start_time: Option<String>,
    #[serde(default)]
    pub ehr_status: OpenEhrEhrStatus,
    pub demographics: Option<OpenEhrDemographics>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "lowercase")]
pub enum Severity {
    Error,
    Warning,
    Information,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ValidationIssue {
    pub severity: Severity,
    pub message: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub location: Option<String>,
}

impl ValidationIssue {
    pub fn error(message: impl Into<String>, location: Option<&str>) -> Self {
        Self { severity: Severity::Error, message: message.into(), location: location.map(String::from) }
    }
    pub fn warning(message: impl Into<String>, location: Option<&str>) -> Self {
        Self { severity: Severity::Warning, message: message.into(), location: location.map(String::from) }
    }
}

/// Outcome of a translation in either direction.
#[derive(Debug, Clone, Serialize)]
pub struct Outcome<T> {
    pub success: bool,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value: Option<T>,
    pub issues: Vec<ValidationIssue>,
}

impl<T> Outcome<T> {
    pub fn ok(value: T, issues: Vec<ValidationIssue>) -> Self {
        Self { success: true, value: Some(value), issues }
    }
    pub fn fail(issues: Vec<ValidationIssue>) -> Self {
        Self { success: false, value: None, issues }
    }
}

pub fn has_error(issues: &[ValidationIssue]) -> bool {
    issues.iter().any(|i| i.severity == Severity::Error)
}
