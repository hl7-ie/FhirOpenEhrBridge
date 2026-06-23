from behave import given, then, when

from fhir_openehr_bridge import TranslationService

_service = TranslationService()


@given("a valid FHIR Patient JSON")
def step_valid_patient(context):
    context.fhir_json = (
        '{"resourceType":"Patient","id":"bdd-1",'
        '"name":[{"family":"Smith","given":["John"]}],'
        '"gender":"male","birthDate":"1980-05-15"}'
    )


@given("a FHIR Observation JSON")
def step_observation(context):
    context.fhir_json = '{"resourceType":"Observation","status":"final"}'


@given("a valid openEHR demographics composition")
def step_valid_composition(context):
    context.composition = {
        "archetypeNodeId": "openEHR-EHR-COMPOSITION.demographics.v1",
        "ehrStatus": {"subjectId": "bdd-2"},
        "demographics": {"familyName": "Smith", "givenName": "John", "gender": "male"},
    }


@given("an openEHR composition without demographics")
def step_composition_without_demographics(context):
    context.composition = {
        "archetypeNodeId": "openEHR-EHR-COMPOSITION.demographics.v1",
        "ehrStatus": {"subjectId": "bdd-3"},
        "demographics": None,
    }


@when("I translate it to openEHR")
def step_translate_to_openehr(context):
    context.result = _service.fhir_to_openehr(context.fhir_json)


@when("I translate it to FHIR")
def step_translate_to_fhir(context):
    context.result = _service.openehr_to_fhir(context.composition)


@then("the translation succeeds")
def step_succeeds(context):
    assert context.result.success, context.result.issues


@then("the translation fails")
def step_fails(context):
    assert not context.result.success


@then('the openEHR demographics family name is "{expected}"')
def step_family_name(context, expected):
    assert context.result.value["demographics"]["familyName"] == expected


@then("the result is a FHIR Bundle containing a Patient")
def step_bundle_with_patient(context):
    bundle = context.result.value
    assert bundle["resourceType"] == "Bundle"
    assert bundle["entry"][0]["resource"]["resourceType"] == "Patient"
