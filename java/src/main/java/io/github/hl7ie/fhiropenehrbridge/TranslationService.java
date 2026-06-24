package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;

/** Translation façade mirroring the .NET ITranslationService. */
public final class TranslationService {

    public TranslationResult<JsonNode> fhirToOpenEhr(String fhirJson) {
        return Mappers.fhirPatientToComposition(fhirJson);
    }

    public TranslationResult<JsonNode> openEhrToFhir(JsonNode composition) {
        return Mappers.compositionToFhirBundle(composition);
    }
}
