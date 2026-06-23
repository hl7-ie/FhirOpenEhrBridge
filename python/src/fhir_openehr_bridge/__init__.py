"""FHIR <-> openEHR translation engine (Python port)."""

from .gender import gender_to_fhir, gender_to_openehr
from .mappers import composition_to_fhir_bundle, fhir_patient_to_composition
from .models import DEMOGRAPHICS_ARCHETYPE, TranslationResult, ValidationIssue
from .service import TranslationService
from .validation import validate_composition, validate_fhir_patient_json

__all__ = [
    "TranslationService",
    "fhir_patient_to_composition",
    "composition_to_fhir_bundle",
    "validate_fhir_patient_json",
    "validate_composition",
    "gender_to_openehr",
    "gender_to_fhir",
    "TranslationResult",
    "ValidationIssue",
    "DEMOGRAPHICS_ARCHETYPE",
]

__version__ = "0.1.0"
