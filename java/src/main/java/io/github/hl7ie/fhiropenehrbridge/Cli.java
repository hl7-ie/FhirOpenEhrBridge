package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;
import java.nio.file.Files;
import java.nio.file.Path;

/** Tiny CLI demo for the Java port. */
public final class Cli {
    private Cli() {}

    public static void main(String[] args) throws Exception {
        System.exit(run(args));
    }

    static int run(String[] args) throws Exception {
        if (args.length < 2) {
            System.err.println("usage: cli <fhir-to-openehr|openehr-to-fhir> <file.json>");
            return 2;
        }
        String direction = args[0];
        String content = Files.readString(Path.of(args[1]));
        TranslationService service = new TranslationService();

        TranslationResult<JsonNode> result;
        switch (direction) {
            case "fhir-to-openehr" -> result = service.fhirToOpenEhr(content);
            case "openehr-to-fhir" -> result = service.openEhrToFhir(Json.MAPPER.readTree(content));
            default -> {
                System.err.println("unknown direction: " + direction);
                return 2;
            }
        }

        System.out.println(Json.MAPPER.writerWithDefaultPrettyPrinter()
            .writeValueAsString(Responses.envelope(result)));
        return result.success() ? 0 : 1;
    }
}
