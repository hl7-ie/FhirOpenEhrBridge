// Gender mapping between FHIR administrative-gender codes and the openEHR local
// value set. FHIR `other` maps to openEHR `intersex` (a documented, lossy
// assumption of the mapping model).

export function genderToOpenEhr(fhirGender?: string): string {
  switch ((fhirGender ?? '').trim().toLowerCase()) {
    case 'male':
      return 'male';
    case 'female':
      return 'female';
    case 'other':
      return 'intersex';
    default:
      return 'unknown';
  }
}

export function genderToFhir(openEhrGender?: string): string | undefined {
  switch ((openEhrGender ?? '').trim().toLowerCase()) {
    case 'male':
      return 'male';
    case 'female':
      return 'female';
    case 'intersex':
      return 'other';
    case 'unknown':
      return 'unknown';
    case '':
      return undefined;
    default:
      return 'unknown';
  }
}
