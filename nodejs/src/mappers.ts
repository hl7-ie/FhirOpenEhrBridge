import { randomUUID } from 'crypto';
import { genderToFhir, genderToOpenEhr } from './genderMap';
import { OpenEhrComposition, TranslationResult } from './models';
import { validateComposition, validateFhirPatientJson } from './validation';

const DEMOGRAPHICS_ARCHETYPE = 'openEHR-EHR-COMPOSITION.demographics.v1';

/** FHIR Patient (JSON) -> openEHR demographics composition. */
export function fhirPatientToComposition(fhirJson: string): TranslationResult<OpenEhrComposition> {
  const { issues, patient } = validateFhirPatientJson(fhirJson);
  if (issues.some((i) => i.severity === 'error') || !patient) {
    return { success: false, issues };
  }

  const primaryName = patient.name?.[0];
  const primaryAddress = patient.address?.[0];

  const composition: OpenEhrComposition = {
    archetypeNodeId: DEMOGRAPHICS_ARCHETYPE,
    name: 'Demographics',
    language: 'en',
    territory: 'GB',
    category: 'persistent',
    startTime: new Date().toISOString(),
    ehrStatus: {
      subjectId: patient.id && patient.id.trim() !== '' ? patient.id : randomUUID(),
      subjectNamespace: 'DEMOGRAPHIC',
      isQueryable: true,
      isModifiable: true,
    },
    demographics: {
      familyName: primaryName?.family,
      givenName: primaryName?.given?.[0],
      gender: genderToOpenEhr(patient.gender),
      birthDate: patient.birthDate,
      identifiers: (patient.identifier ?? [])
        .filter((id) => id.value && id.value.trim() !== '')
        .map((id) => ({
          id: id.value,
          issuer: id.system,
          type: id.type?.text ?? id.type?.coding?.[0]?.code,
        })),
      address: primaryAddress
        ? {
            line: primaryAddress.line?.[0],
            city: primaryAddress.city,
            postalCode: primaryAddress.postalCode,
            country: primaryAddress.country,
          }
        : null,
    },
  };

  return { success: true, value: composition, issues };
}

/** openEHR demographics composition -> FHIR Bundle (collection) JSON. */
export function compositionToFhirBundle(
  composition: OpenEhrComposition,
): TranslationResult<string> {
  const { issues } = validateComposition(composition);
  if (issues.some((i) => i.severity === 'error')) {
    return { success: false, issues };
  }

  const d = composition.demographics;
  const patientId =
    composition.ehrStatus?.subjectId && composition.ehrStatus.subjectId.trim() !== ''
      ? composition.ehrStatus.subjectId
      : randomUUID();

  const patient: Record<string, unknown> = { resourceType: 'Patient', id: patientId };

  if (d.familyName || d.givenName) {
    const name: Record<string, unknown> = { use: 'official' };
    if (d.familyName) name.family = d.familyName;
    if (d.givenName) name.given = [d.givenName];
    patient.name = [name];
  }

  const gender = genderToFhir(d.gender);
  if (gender) patient.gender = gender;
  if (d.birthDate) patient.birthDate = d.birthDate;

  const identifiers = (d.identifiers ?? [])
    .filter((i) => i.id && i.id.trim() !== '')
    .map((i) => {
      const fi: Record<string, unknown> = { system: i.issuer, value: i.id };
      if (i.type) fi.type = { text: i.type };
      return fi;
    });
  if (identifiers.length > 0) patient.identifier = identifiers;

  if (d.address) {
    const address: Record<string, unknown> = {};
    if (d.address.line) address.line = [d.address.line];
    if (d.address.city) address.city = d.address.city;
    if (d.address.postalCode) address.postalCode = d.address.postalCode;
    if (d.address.country) address.country = d.address.country;
    patient.address = [address];
  }

  const bundle = {
    resourceType: 'Bundle',
    id: randomUUID(),
    type: 'collection',
    timestamp: composition.startTime ?? new Date().toISOString(),
    entry: [{ fullUrl: `urn:uuid:${patientId}`, resource: patient }],
  };

  return { success: true, value: JSON.stringify(bundle), issues };
}
