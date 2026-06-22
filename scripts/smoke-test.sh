#!/usr/bin/env bash
# Smoke-test the locally running bridge API (docker compose up).
# Usage: ./scripts/smoke-test.sh [BASE_URL]   (default http://localhost:8088)
set -euo pipefail

BASE_URL="${1:-http://localhost:8088}"
fail() { echo "FAIL: $1" >&2; exit 1; }

echo "==> Health"
curl -fsS "${BASE_URL}/health" | grep -q "Healthy" || fail "health check failed"
echo "    ok"

echo "==> FHIR -> openEHR"
F2O=$(curl -fsS -X POST "${BASE_URL}/api/translate/fhir-to-openehr" \
  -H "Content-Type: application/fhir+json" \
  -d '{"resourceType":"Patient","id":"smoke-1","name":[{"family":"Smith","given":["John"]}],"gender":"male","birthDate":"1980-05-15"}')
echo "$F2O" | grep -q '"success":true' || fail "fhir-to-openehr did not succeed: $F2O"
echo "$F2O" | grep -q '"familyName":"Smith"' || fail "fhir-to-openehr missing family name"
echo "    ok"

echo "==> openEHR -> FHIR"
O2F=$(curl -fsS -X POST "${BASE_URL}/api/translate/openehr-to-fhir" \
  -H "Content-Type: application/json" \
  -d '{"archetypeNodeId":"openEHR-EHR-COMPOSITION.demographics.v1","ehrStatus":{"subjectId":"smoke-1"},"demographics":{"familyName":"Smith","givenName":"John","gender":"male"}}')
echo "$O2F" | grep -q '"resourceType":"Bundle"' || fail "openehr-to-fhir did not return a Bundle: $O2F"
echo "    ok"

echo "==> Negative case (Observation should 400)"
CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "${BASE_URL}/api/translate/fhir-to-openehr" \
  -H "Content-Type: application/json" -d '{"resourceType":"Observation","status":"final"}')
[ "$CODE" = "400" ] || fail "expected 400 for Observation, got $CODE"
echo "    ok (400)"

echo "All smoke tests passed against ${BASE_URL}"
