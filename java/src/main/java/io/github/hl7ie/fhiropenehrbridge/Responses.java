package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.node.ObjectNode;

/** Builds the {success, result, issues} response envelope shared by the API and CLI. */
public final class Responses {
    private Responses() {}

    public static ObjectNode envelope(TranslationResult<JsonNode> result) {
        ObjectNode node = Json.MAPPER.createObjectNode();
        node.put("success", result.success());
        node.set("result", result.value() != null ? result.value() : Json.MAPPER.nullNode());
        node.set("issues", Json.MAPPER.valueToTree(result.issues()));
        return node;
    }
}
