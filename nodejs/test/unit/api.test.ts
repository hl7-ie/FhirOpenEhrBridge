import request from 'supertest';
import { createApp } from '../../src/server';

const app = createApp();

describe('HTTP API', () => {
  it('GET /health returns healthy', async () => {
    const res = await request(app).get('/health');
    expect(res.status).toBe(200);
    expect(res.body.status).toBe('Healthy');
  });

  it('POST /api/translate/fhir-to-openehr translates a Patient', async () => {
    const res = await request(app)
      .post('/api/translate/fhir-to-openehr')
      .set('Content-Type', 'application/fhir+json')
      .send(JSON.stringify({ resourceType: 'Patient', id: 'a1', name: [{ family: 'Doe', given: ['Jane'] }], gender: 'female' }));
    expect(res.status).toBe(200);
    expect(res.body.success).toBe(true);
    expect(res.body.result.demographics.familyName).toBe('Doe');
  });

  it('POST /api/translate/fhir-to-openehr rejects a non-Patient with 400', async () => {
    const res = await request(app)
      .post('/api/translate/fhir-to-openehr')
      .set('Content-Type', 'application/json')
      .send(JSON.stringify({ resourceType: 'Observation', status: 'final' }));
    expect(res.status).toBe(400);
  });

  it('POST /api/translate/openehr-to-fhir returns a Bundle', async () => {
    const res = await request(app)
      .post('/api/translate/openehr-to-fhir')
      .set('Content-Type', 'application/json')
      .send({
        archetypeNodeId: 'openEHR-EHR-COMPOSITION.demographics.v1',
        ehrStatus: { subjectId: 'a2' },
        demographics: { familyName: 'Doe', givenName: 'Jane', gender: 'female' },
      });
    expect(res.status).toBe(200);
    expect(res.body.result.resourceType).toBe('Bundle');
  });
});
