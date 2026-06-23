import { compositionToFhirBundle, fhirPatientToComposition } from './mappers';
import { OpenEhrComposition, TranslationResult } from './models';

/**
 * Facade selecting the appropriate mapper for an inbound payload. Mirrors the
 * .NET `ITranslationService`. For the demographics proof-of-concept there is a
 * single mapper per direction.
 */
export class TranslationService {
  fhirToOpenEhr(fhirJson: string): TranslationResult<OpenEhrComposition> {
    return fhirPatientToComposition(fhirJson);
  }

  openEhrToFhir(composition: OpenEhrComposition): TranslationResult<string> {
    if (!composition) {
      return { success: false, issues: [{ severity: 'error', message: 'openEHR composition is null.' }] };
    }
    return compositionToFhirBundle(composition);
  }
}
