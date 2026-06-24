//! FHIR <-> openEHR translation engine (Rust port).
//!
//! Part of the polyglot monorepo; behaviour matches the shared conformance
//! contract in `docs/POLYGLOT.md`.

pub mod gender;
pub mod mappers;
pub mod models;
pub mod service;
pub mod validation;

pub use mappers::{composition_to_fhir_bundle, fhir_patient_to_composition};
pub use models::*;
pub use service::TranslationService;
