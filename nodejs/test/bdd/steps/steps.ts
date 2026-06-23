import assert from 'assert';
import { Given, When, Then, Before } from '@cucumber/cucumber';
import { TranslationService } from '../../../src/translationService';
import { OpenEhrComposition, TranslationResult } from '../../../src/models';

interface World {
  fhirJson?: string;
  composition?: OpenEhrComposition;
  fhirResult?: TranslationResult<OpenEhrComposition>;
  bundleResult?: TranslationResult<string>;
}

const service = new TranslationService();
let world: World;

Before(function () {
  world = {};
});

Given('a valid FHIR Patient JSON', function () {
  world.fhirJson = JSON.stringify({
    resourceType: 'Patient',
    id: 'bdd-1',
    name: [{ family: 'Smith', given: ['John'] }],
    gender: 'male',
    birthDate: '1980-05-15',
  });
});

Given('a FHIR Observation JSON', function () {
  world.fhirJson = JSON.stringify({ resourceType: 'Observation', status: 'final' });
});

Given('a valid openEHR demographics composition', function () {
  world.composition = {
    archetypeNodeId: 'openEHR-EHR-COMPOSITION.demographics.v1',
    name: 'Demographics',
    language: 'en',
    territory: 'GB',
    category: 'persistent',
    ehrStatus: { subjectId: 'bdd-2', subjectNamespace: 'DEMOGRAPHIC', isQueryable: true, isModifiable: true },
    demographics: { familyName: 'Smith', givenName: 'John', gender: 'male', identifiers: [] },
  };
});

Given('an openEHR composition without demographics', function () {
  world.composition = {
    archetypeNodeId: 'openEHR-EHR-COMPOSITION.demographics.v1',
    name: 'Demographics',
    language: 'en',
    territory: 'GB',
    category: 'persistent',
    ehrStatus: { subjectId: 'bdd-3', subjectNamespace: 'DEMOGRAPHIC', isQueryable: true, isModifiable: true },
    // @ts-expect-error intentionally invalid for the scenario
    demographics: null,
  };
});

When('I translate it to openEHR', function () {
  world.fhirResult = service.fhirToOpenEhr(world.fhirJson as string);
});

When('I translate it to FHIR', function () {
  world.bundleResult = service.openEhrToFhir(world.composition as OpenEhrComposition);
});

Then('the translation succeeds', function () {
  const ok = world.fhirResult?.success ?? world.bundleResult?.success;
  assert.strictEqual(ok, true);
});

Then('the translation fails', function () {
  const ok = world.fhirResult?.success ?? world.bundleResult?.success;
  assert.strictEqual(ok, false);
});

Then('the openEHR demographics family name is {string}', function (expected: string) {
  assert.strictEqual(world.fhirResult?.value?.demographics.familyName, expected);
});

Then('the result is a FHIR Bundle containing a Patient', function () {
  const bundle = JSON.parse(world.bundleResult?.value as string);
  assert.strictEqual(bundle.resourceType, 'Bundle');
  assert.strictEqual(bundle.entry[0].resource.resourceType, 'Patient');
});
