"""Translation façade mirroring the .NET ITranslationService."""

from __future__ import annotations

from typing import Any

from .mappers import composition_to_fhir_bundle, fhir_patient_to_composition
from .models import TranslationResult, ValidationIssue


class TranslationService:
    def fhir_to_openehr(self, fhir_json: str) -> TranslationResult:
        return fhir_patient_to_composition(fhir_json)

    def openehr_to_fhir(self, composition: dict[str, Any] | None) -> TranslationResult:
        if composition is None:
            return TranslationResult(False, None, [ValidationIssue("error", "openEHR composition is null.")])
        return composition_to_fhir_bundle(composition)
