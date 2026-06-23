import json

import pytest

from fhir_openehr_bridge import (
    TranslationService,
    composition_to_fhir_bundle,
    fhir_patient_to_composition,
)

PATIENT_JSON = json.dumps(
    {
        "resourceType": "Patient",
        "id": "example-123",
        "identifier": [
            {"system": "https://fhir.nhs.uk/Id/nhs-number", "value": "9876543210", "type": {"text": "NHS"}}
        ],
        "name": [{"use": "official", "family": "Smith", "given": ["John"]}],
        "gender": "male",
        "birthDate": "1980-05-15",
        "address": [{"line": ["10 Downing Street"], "city": "London", "postalCode": "SW1A 2AA", "country": "GB"}],
    }
)


def sample_composition():
    return {
        "archetypeNodeId": "openEHR-EHR-COMPOSITION.demographics.v1",
        "ehrStatus": {"subjectId": "example-123"},
        "demographics": {
            "familyName": "Smith",
            "givenName": "John",
            "gender": "male",
            "birthDate": "1980-05-15",
            "identifiers": [{"id": "9876543210", "issuer": "https://fhir.nhs.uk/Id/nhs-number", "type": "NHS"}],
            "address": {"line": "10 Downing Street", "city": "London", "postalCode": "SW1A 2AA", "country": "GB"},
        },
    }


def test_fhir_patient_to_composition():
    result = fhir_patient_to_composition(PATIENT_JSON)
    assert result.success
    d = result.value["demographics"]
    assert d["familyName"] == "Smith"
    assert d["givenName"] == "John"
    assert d["gender"] == "male"
    assert d["birthDate"] == "1980-05-15"
    assert result.value["ehrStatus"]["subjectId"] == "example-123"
    assert d["identifiers"][0]["id"] == "9876543210"
    assert d["address"]["city"] == "London"


def test_rejects_non_patient():
    result = fhir_patient_to_composition(json.dumps({"resourceType": "Observation", "status": "final"}))
    assert not result.success
    assert any(i.severity == "error" for i in result.issues)


@pytest.mark.parametrize("bad", ["", "   ", "{ not json"])
def test_rejects_invalid_json(bad):
    assert not fhir_patient_to_composition(bad).success


def test_composition_to_fhir_bundle():
    result = composition_to_fhir_bundle(sample_composition())
    assert result.success
    bundle = result.value
    assert bundle["resourceType"] == "Bundle"
    assert bundle["type"] == "collection"
    patient = bundle["entry"][0]["resource"]
    assert patient["id"] == "example-123"
    assert patient["gender"] == "male"
    assert patient["identifier"][0]["value"] == "9876543210"


def test_rejects_missing_demographics():
    comp = sample_composition()
    comp["demographics"] = None
    assert not composition_to_fhir_bundle(comp).success


def test_round_trip():
    comp = fhir_patient_to_composition(PATIENT_JSON).value
    bundle = composition_to_fhir_bundle(comp).value
    patient = bundle["entry"][0]["resource"]
    assert patient["id"] == "example-123"
    assert patient["name"][0]["family"] == "Smith"
    assert patient["identifier"][0]["value"] == "9876543210"


def test_service_dispatch():
    svc = TranslationService()
    assert svc.fhir_to_openehr(PATIENT_JSON).success
    assert svc.openehr_to_fhir(sample_composition()).success
