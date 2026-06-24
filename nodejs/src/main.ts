import { createApp } from './server';

const port = Number(process.env.PORT ?? 8080);
createApp().listen(port, () => {
  // eslint-disable-next-line no-console
  console.log(`FHIR-OpenEHR-Bridge (Node) listening on :${port}`);
});
