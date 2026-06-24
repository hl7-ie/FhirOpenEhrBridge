"""Gender mapping between FHIR and openEHR value sets."""

from __future__ import annotations

from typing import Optional


def gender_to_openehr(fhir_gender: Optional[str]) -> str:
    match (fhir_gender or "").strip().lower():
        case "male":
            return "male"
        case "female":
            return "female"
        case "other":
            return "intersex"
        case _:
            return "unknown"


def gender_to_fhir(openehr_gender: Optional[str]) -> Optional[str]:
    """Returns ``None`` for empty input (meaning 'do not set gender')."""
    match (openehr_gender or "").strip().lower():
        case "male":
            return "male"
        case "female":
            return "female"
        case "intersex":
            return "other"
        case "unknown":
            return "unknown"
        case "":
            return None
        case _:
            return "unknown"
