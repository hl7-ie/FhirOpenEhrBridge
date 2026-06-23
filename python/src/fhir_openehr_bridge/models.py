"""Shared constants and result types for the Python port.

Compositions and FHIR payloads are represented as plain ``dict`` objects using
the canonical camelCase keys (see docs/POLYGLOT.md), which map directly to JSON.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Optional

DEMOGRAPHICS_ARCHETYPE = "openEHR-EHR-COMPOSITION.demographics.v1"


@dataclass
class ValidationIssue:
    """A single problem found while validating a payload."""

    severity: str  # "error" | "warning" | "information"
    message: str
    location: Optional[str] = None

    def to_dict(self) -> dict[str, Any]:
        d: dict[str, Any] = {"severity": self.severity, "message": self.message}
        if self.location:
            d["location"] = self.location
        return d


@dataclass
class TranslationResult:
    """Outcome of a translation in either direction."""

    success: bool
    value: Any
    issues: list[ValidationIssue]

    def to_response(self) -> dict[str, Any]:
        return {
            "success": self.success,
            "result": self.value,
            "issues": [i.to_dict() for i in self.issues],
        }


def has_error(issues: list[ValidationIssue]) -> bool:
    return any(i.severity == "error" for i in issues)
