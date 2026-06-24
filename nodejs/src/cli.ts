#!/usr/bin/env node
import { readFileSync } from 'fs';
import { TranslationService } from './translationService';

// Tiny CLI demo: translate a file in either direction.
//   fhir-openehr-bridge fhir-to-openehr <patient.json>
//   fhir-openehr-bridge openehr-to-fhir <composition.json>
function main(argv: string[]): number {
  const [direction, file] = argv;
  if (!direction || !file) {
    console.error('usage: fhir-openehr-bridge <fhir-to-openehr|openehr-to-fhir> <file.json>');
    return 2;
  }

  const service = new TranslationService();
  const content = readFileSync(file, 'utf-8');

  if (direction === 'fhir-to-openehr') {
    const result = service.fhirToOpenEhr(content);
    console.log(JSON.stringify({ success: result.success, result: result.value ?? null, issues: result.issues }, null, 2));
    return result.success ? 0 : 1;
  }

  if (direction === 'openehr-to-fhir') {
    const result = service.openEhrToFhir(JSON.parse(content));
    console.log(
      JSON.stringify(
        { success: result.success, result: result.value ? JSON.parse(result.value) : null, issues: result.issues },
        null,
        2,
      ),
    );
    return result.success ? 0 : 1;
  }

  console.error(`unknown direction: ${direction}`);
  return 2;
}

process.exit(main(process.argv.slice(2)));
