"""Inbound payload validation."""

from __future__ import annotations

import json
from typing import Any, Optional

from .models import ValidationIssue


def validate_fhir_patient_json(
    payload: str,
) -> tuple[list[ValidationIssue], Optional[dict[str, Any]]]:
    """Parses and validates inbound FHIR Patient JSON."""
    issues: list[ValidationIssue] = []

    if not payload or not payload.strip():
        issues.append(ValidationIssue("error", "FHIR payload is empty."))
        return issues, None

    try:
        parsed = json.loads(payload)
    except json.JSONDecodeError as exc:
        issues.append(ValidationIssue("error", f"Payload is not valid FHIR JSON: {exc}"))
        return issues, None

    if not isinstance(parsed, dict):
        issues.append(ValidationIssue("error", "Payload did not parse to a FHIR resource."))
        return issues, None

    if parsed.get("resourceType") != "Patient":
        issues.append(
            ValidationIssue(
                "error",
                f"Expected a FHIR 'Patient' resource but received '{parsed.get('resourceType')}'.",
                "resourceType",
            )
        )
        return issues, None

    return issues, parsed


def validate_composition(composition: Optional[dict[str, Any]]) -> list[ValidationIssue]:
    """Validates an openEHR composition before mapping."""
    issues: list[ValidationIssue] = []

    if composition is None:
        issues.append(ValidationIssue("error", "openEHR composition is null."))
        return issues

    if not (composition.get("archetypeNodeId") or "").strip():
        issues.append(
            ValidationIssue("error", "Composition is missing 'archetypeNodeId'.", "archetypeNodeId")
        )

    demographics = composition.get("demographics")
    if not demographics:
        issues.append(
            ValidationIssue(
                "error", "Composition does not contain a demographics payload.", "demographics"
            )
        )
    elif not demographics.get("familyName") and not demographics.get("givenName"):
        issues.append(
            ValidationIssue(
                "warning",
                "Demographics contain neither a family name nor a given name.",
                "demographics.familyName",
            )
        )

    ehr_status = composition.get("ehrStatus") or {}
    if not (ehr_status.get("subjectId") or "").strip():
        issues.append(
            ValidationIssue(
                "warning",
                "EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.",
                "ehrStatus.subjectId",
            )
        )

    return issues
