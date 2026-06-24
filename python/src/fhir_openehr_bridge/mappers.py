"""Bidirectional mappers."""

from __future__ import annotations

import uuid
from datetime import datetime, timezone
from typing import Any

from .gender import gender_to_fhir, gender_to_openehr
from .models import DEMOGRAPHICS_ARCHETYPE, TranslationResult, has_error
from .validation import validate_composition, validate_fhir_patient_json


def fhir_patient_to_composition(fhir_json: str) -> TranslationResult:
    """FHIR Patient JSON -> openEHR demographics composition (dict)."""
    issues, patient = validate_fhir_patient_json(fhir_json)
    if has_error(issues) or patient is None:
        return TranslationResult(False, None, issues)

    names = patient.get("name") or []
    name0 = names[0] if names else {}
    addresses = patient.get("address") or []
    address0 = addresses[0] if addresses else None

    identifiers = []
    for ident in patient.get("identifier") or []:
        value = ident.get("value")
        if not value or not str(value).strip():
            continue
        type_ = None
        t = ident.get("type")
        if isinstance(t, dict):
            type_ = t.get("text")
            if not type_:
                coding = t.get("coding") or []
                if coding:
                    type_ = coding[0].get("code")
        identifiers.append({"id": value, "issuer": ident.get("system"), "type": type_})

    demographics: dict[str, Any] = {
        "familyName": name0.get("family"),
        "givenName": (name0.get("given") or [None])[0],
        "gender": gender_to_openehr(patient.get("gender")),
        "birthDate": patient.get("birthDate"),
        "identifiers": identifiers,
    }
    if address0 is not None:
        demographics["address"] = {
            "line": (address0.get("line") or [None])[0],
            "city": address0.get("city"),
            "postalCode": address0.get("postalCode"),
            "country": address0.get("country"),
        }

    subject = patient.get("id") or str(uuid.uuid4())
    composition = {
        "archetypeNodeId": DEMOGRAPHICS_ARCHETYPE,
        "name": "Demographics",
        "language": "en",
        "territory": "GB",
        "category": "persistent",
        "startTime": datetime.now(timezone.utc).isoformat(),
        "ehrStatus": {
            "subjectId": subject,
            "subjectNamespace": "DEMOGRAPHIC",
            "isQueryable": True,
            "isModifiable": True,
        },
        "demographics": demographics,
    }
    return TranslationResult(True, composition, issues)


def composition_to_fhir_bundle(composition: dict[str, Any]) -> TranslationResult:
    """openEHR demographics composition (dict) -> FHIR Bundle (dict)."""
    issues = validate_composition(composition)
    if has_error(issues):
        return TranslationResult(False, None, issues)

    d = composition.get("demographics") or {}
    ehr_status = composition.get("ehrStatus") or {}
    patient_id = (ehr_status.get("subjectId") or "").strip() or str(uuid.uuid4())

    patient: dict[str, Any] = {"resourceType": "Patient", "id": patient_id}

    if d.get("familyName") or d.get("givenName"):
        name: dict[str, Any] = {"use": "official"}
        if d.get("familyName"):
            name["family"] = d["familyName"]
        if d.get("givenName"):
            name["given"] = [d["givenName"]]
        patient["name"] = [name]

    gender = gender_to_fhir(d.get("gender"))
    if gender:
        patient["gender"] = gender
    if d.get("birthDate"):
        patient["birthDate"] = d["birthDate"]

    identifiers = []
    for ident in d.get("identifiers") or []:
        if not ident.get("id") or not str(ident["id"]).strip():
            continue
        fi: dict[str, Any] = {"system": ident.get("issuer"), "value": ident["id"]}
        if ident.get("type"):
            fi["type"] = {"text": ident["type"]}
        identifiers.append(fi)
    if identifiers:
        patient["identifier"] = identifiers

    address = d.get("address")
    if address:
        fa: dict[str, Any] = {}
        if address.get("line"):
            fa["line"] = [address["line"]]
        if address.get("city"):
            fa["city"] = address["city"]
        if address.get("postalCode"):
            fa["postalCode"] = address["postalCode"]
        if address.get("country"):
            fa["country"] = address["country"]
        patient["address"] = [fa]

    bundle = {
        "resourceType": "Bundle",
        "id": str(uuid.uuid4()),
        "type": "collection",
        "timestamp": composition.get("startTime") or datetime.now(timezone.utc).isoformat(),
        "entry": [{"fullUrl": f"urn:uuid:{patient_id}", "resource": patient}],
    }
    return TranslationResult(True, bundle, issues)
