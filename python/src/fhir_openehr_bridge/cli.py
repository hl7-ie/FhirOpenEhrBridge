"""Tiny CLI demo for the Python port."""

from __future__ import annotations

import json
import sys

from .service import TranslationService


def main(argv: list[str] | None = None) -> int:
    argv = argv if argv is not None else sys.argv[1:]
    if len(argv) < 2:
        print("usage: fhir-openehr-bridge <fhir-to-openehr|openehr-to-fhir> <file.json>", file=sys.stderr)
        return 2

    direction, path = argv[0], argv[1]
    with open(path, "r", encoding="utf-8") as fh:
        content = fh.read()

    service = TranslationService()
    if direction == "fhir-to-openehr":
        result = service.fhir_to_openehr(content)
    elif direction == "openehr-to-fhir":
        result = service.openehr_to_fhir(json.loads(content))
    else:
        print(f"unknown direction: {direction}", file=sys.stderr)
        return 2

    print(json.dumps(result.to_response(), indent=2))
    return 0 if result.success else 1


if __name__ == "__main__":
    raise SystemExit(main())
