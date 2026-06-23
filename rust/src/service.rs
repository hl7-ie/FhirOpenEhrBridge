//! Translation façade mirroring the .NET ITranslationService.

use crate::mappers::{composition_to_fhir_bundle, fhir_patient_to_composition};
use crate::models::{OpenEhrComposition, Outcome};
use serde_json::Value;

#[derive(Debug, Default, Clone, Copy)]
pub struct TranslationService;

impl TranslationService {
    pub fn new() -> Self {
        Self
    }

    pub fn fhir_to_openehr(&self, fhir_json: &str) -> Outcome<OpenEhrComposition> {
        fhir_patient_to_composition(fhir_json)
    }

    pub fn openehr_to_fhir(&self, composition: &OpenEhrComposition) -> Outcome<Value> {
        composition_to_fhir_bundle(composition)
    }
}
