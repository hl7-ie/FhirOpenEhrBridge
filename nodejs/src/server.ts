import express, { Express } from 'express';
import { TranslationService } from './translationService';
import { OpenEhrComposition } from './models';

/** Builds the Express app exposing the translation endpoints. */
export function createApp(): Express {
  const app = express();
  const service = new TranslationService();

  app.get('/health', (_req, res) => {
    res.json({ status: 'Healthy', service: 'FHIR-OpenEHR-Bridge' });
  });

  // The body is raw FHIR JSON (text), so we map any content type to a string.
  app.post(
    '/api/translate/fhir-to-openehr',
    express.text({ type: () => true, limit: '5mb' }),
    (req, res) => {
      const body = typeof req.body === 'string' ? req.body : '';
      const result = service.fhirToOpenEhr(body);
      res.status(result.success ? 200 : 400).json({
        success: result.success,
        result: result.value ?? null,
        issues: result.issues,
      });
    },
  );

  app.post(
    '/api/translate/openehr-to-fhir',
    express.json({ type: () => true, limit: '5mb' }),
    (req, res) => {
      const result = service.openEhrToFhir(req.body as OpenEhrComposition);
      if (!result.success) {
        res.status(400).json({ success: false, result: null, issues: result.issues });
        return;
      }
      // Re-parse so the Bundle is emitted as a JSON object, not an escaped string.
      res.status(200).json({
        success: true,
        result: JSON.parse(result.value as string),
        issues: result.issues,
      });
    },
  );

  return app;
}
