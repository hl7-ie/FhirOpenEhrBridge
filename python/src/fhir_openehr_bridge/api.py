"""Stdlib HTTP API for the Python port (no third-party runtime dependencies)."""

from __future__ import annotations

import json
import os
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

from .service import TranslationService

_service = TranslationService()


class BridgeHandler(BaseHTTPRequestHandler):
    def _send(self, status: int, body: dict) -> None:
        payload = json.dumps(body).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    def _read_body(self) -> bytes:
        length = int(self.headers.get("Content-Length", 0))
        return self.rfile.read(length) if length else b""

    def do_GET(self) -> None:  # noqa: N802
        if self.path == "/health":
            self._send(200, {"status": "Healthy", "service": "FHIR-OpenEHR-Bridge"})
        else:
            self._send(404, {"error": "not found"})

    def do_POST(self) -> None:  # noqa: N802
        body = self._read_body().decode("utf-8")
        if self.path == "/api/translate/fhir-to-openehr":
            result = _service.fhir_to_openehr(body)
            self._send(200 if result.success else 400, result.to_response())
        elif self.path == "/api/translate/openehr-to-fhir":
            try:
                composition = json.loads(body) if body else None
            except json.JSONDecodeError as exc:
                self._send(400, {"success": False, "result": None,
                                 "issues": [{"severity": "error", "message": f"Invalid JSON body: {exc}"}]})
                return
            result = _service.openehr_to_fhir(composition)
            self._send(200 if result.success else 400, result.to_response())
        else:
            self._send(404, {"error": "not found"})

    def log_message(self, *args) -> None:  # silence default logging
        pass


def serve(port: int | None = None) -> None:
    port = port or int(os.environ.get("PORT", "8080"))
    server = ThreadingHTTPServer(("0.0.0.0", port), BridgeHandler)
    print(f"FHIR-OpenEHR-Bridge (Python) listening on :{port}")
    server.serve_forever()


if __name__ == "__main__":
    serve()
