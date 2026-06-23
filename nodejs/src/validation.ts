import { FhirPatient, OpenEhrComposition, ValidationIssue } from './models';

/** Validates that a string is well-formed FHIR JSON for the expected resource type. */
export function validateFhirPatientJson(json: string): {
  issues: ValidationIssue[];
  patient?: FhirPatient;
} {
  const issues: ValidationIssue[] = [];

  if (!json || json.trim() === '') {
    issues.push({ severity: 'error', message: 'FHIR payload is empty.' });
    return { issues };
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch (err) {
    issues.push({
      severity: 'error',
      message: `Payload is not valid FHIR JSON: ${(err as Error).message}`,
    });
    return { issues };
  }

  if (typeof parsed !== 'object' || parsed === null) {
    issues.push({ severity: 'error', message: 'Payload did not parse to a FHIR resource.' });
    return { issues };
  }

  const resource = parsed as FhirPatient;
  if (resource.resourceType !== 'Patient') {
    issues.push({
      severity: 'error',
      message: `Expected a FHIR 'Patient' resource but received '${resource.resourceType}'.`,
      location: 'resourceType',
    });
    return { issues };
  }

  return { issues, patient: resource };
}

/** Validates the structure of an openEHR composition before mapping. */
export function validateComposition(composition: OpenEhrComposition | null | undefined): {
  issues: ValidationIssue[];
} {
  const issues: ValidationIssue[] = [];

  if (!composition) {
    issues.push({ severity: 'error', message: 'openEHR composition is null.' });
    return { issues };
  }

  if (!composition.archetypeNodeId) {
    issues.push({
      severity: 'error',
      message: "Composition is missing 'archetypeNodeId'.",
      location: 'archetypeNodeId',
    });
  }

  if (!composition.demographics) {
    issues.push({
      severity: 'error',
      message: 'Composition does not contain a demographics payload.',
      location: 'demographics',
    });
  } else if (!composition.demographics.familyName && !composition.demographics.givenName) {
    issues.push({
      severity: 'warning',
      message: 'Demographics contain neither a family name nor a given name.',
      location: 'demographics.familyName',
    });
  }

  if (!composition.ehrStatus || !composition.ehrStatus.subjectId) {
    issues.push({
      severity: 'warning',
      message:
        'EHR_STATUS has no subject id; the produced FHIR Patient will be assigned a generated id.',
      location: 'ehrStatus.subjectId',
    });
  }

  return { issues };
}
