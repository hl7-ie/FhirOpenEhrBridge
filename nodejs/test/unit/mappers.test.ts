import { fhirPatientToComposition, compositionToFhirBundle } from '../../src/mappers';
import { OpenEhrComposition } from '../../src/models';

const patientJson = JSON.stringify({
  resourceType: 'Patient',
  id: 'example-123',
  identifier: [
    { system: 'https://fhir.nhs.uk/Id/nhs-number', value: '9876543210', type: { text: 'NHS' } },
  ],
  name: [{ use: 'official', family: 'Smith', given: ['John'] }],
  gender: 'male',
  birthDate: '1980-05-15',
  address: [{ line: ['10 Downing Street'], city: 'London', postalCode: 'SW1A 2AA', country: 'GB' }],
});

function sampleComposition(): OpenEhrComposition {
  return {
    archetypeNodeId: 'openEHR-EHR-COMPOSITION.demographics.v1',
    name: 'Demographics',
    language: 'en',
    territory: 'GB',
    category: 'persistent',
    ehrStatus: { subjectId: 'example-123', subjectNamespace: 'DEMOGRAPHIC', isQueryable: true, isModifiable: true },
    demographics: {
      familyName: 'Smith',
      givenName: 'John',
      gender: 'male',
      birthDate: '1980-05-15',
      identifiers: [{ id: '9876543210', issuer: 'https://fhir.nhs.uk/Id/nhs-number', type: 'NHS' }],
      address: { line: '10 Downing Street', city: 'London', postalCode: 'SW1A 2AA', country: 'GB' },
    },
  };
}

describe('fhirPatientToComposition', () => {
  it('maps a valid Patient to demographics', () => {
    const result = fhirPatientToComposition(patientJson);
    expect(result.success).toBe(true);
    expect(result.value?.demographics.familyName).toBe('Smith');
    expect(result.value?.demographics.givenName).toBe('John');
    expect(result.value?.demographics.gender).toBe('male');
    expect(result.value?.demographics.birthDate).toBe('1980-05-15');
    expect(result.value?.ehrStatus.subjectId).toBe('example-123');
  });

  it('maps identifiers and address', () => {
    const result = fhirPatientToComposition(patientJson);
    expect(result.value?.demographics.identifiers).toHaveLength(1);
    expect(result.value?.demographics.identifiers[0].id).toBe('9876543210');
    expect(result.value?.demographics.address?.city).toBe('London');
  });

  it('rejects a non-Patient resource', () => {
    const result = fhirPatientToComposition(JSON.stringify({ resourceType: 'Observation', status: 'final' }));
    expect(result.success).toBe(false);
    expect(result.issues.some((i) => i.severity === 'error')).toBe(true);
  });

  it.each(['', '   ', '{ not json'])('rejects invalid input %p', (bad) => {
    const result = fhirPatientToComposition(bad);
    expect(result.success).toBe(false);
    expect(result.issues.length).toBeGreaterThan(0);
  });
});

describe('compositionToFhirBundle', () => {
  it('produces a Bundle with a Patient', () => {
    const result = compositionToFhirBundle(sampleComposition());
    expect(result.success).toBe(true);
    const bundle = JSON.parse(result.value as string);
    expect(bundle.resourceType).toBe('Bundle');
    expect(bundle.type).toBe('collection');
    const patient = bundle.entry[0].resource;
    expect(patient.resourceType).toBe('Patient');
    expect(patient.id).toBe('example-123');
    expect(patient.name[0].family).toBe('Smith');
    expect(patient.gender).toBe('male');
    expect(patient.identifier[0].value).toBe('9876543210');
  });

  it('rejects a composition without demographics', () => {
    const comp = sampleComposition();
    // @ts-expect-error intentionally invalid
    comp.demographics = null;
    const result = compositionToFhirBundle(comp);
    expect(result.success).toBe(false);
  });
});

describe('round trip', () => {
  it('preserves core fields', () => {
    const toOpenEhr = fhirPatientToComposition(patientJson);
    const toFhir = compositionToFhirBundle(toOpenEhr.value as OpenEhrComposition);
    const patient = JSON.parse(toFhir.value as string).entry[0].resource;
    expect(patient.id).toBe('example-123');
    expect(patient.name[0].family).toBe('Smith');
    expect(patient.gender).toBe('male');
    expect(patient.identifier[0].value).toBe('9876543210');
  });
});
