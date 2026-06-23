package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.JsonNode;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;
import java.io.IOException;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;

/** HTTP API for the Java port, built on the JDK's bundled HttpServer (no Spring). */
public final class ApiServer {

    private static final TranslationService SERVICE = new TranslationService();

    public static void main(String[] args) throws IOException {
        int port = Integer.parseInt(System.getenv().getOrDefault("PORT", "8080"));
        HttpServer server = HttpServer.create(new InetSocketAddress("0.0.0.0", port), 0);

        server.createContext("/health", exchange -> {
            JsonNode body = Json.MAPPER.createObjectNode()
                .put("status", "Healthy")
                .put("service", "FHIR-OpenEHR-Bridge");
            respond(exchange, 200, body);
        });

        server.createContext("/api/translate/fhir-to-openehr", exchange -> {
            if (!"POST".equals(exchange.getRequestMethod())) {
                exchange.sendResponseHeaders(405, -1);
                return;
            }
            String body = new String(exchange.getRequestBody().readAllBytes(), StandardCharsets.UTF_8);
            TranslationResult<JsonNode> result = SERVICE.fhirToOpenEhr(body);
            respond(exchange, result.success() ? 200 : 400, Responses.envelope(result));
        });

        server.createContext("/api/translate/openehr-to-fhir", exchange -> {
            if (!"POST".equals(exchange.getRequestMethod())) {
                exchange.sendResponseHeaders(405, -1);
                return;
            }
            String body = new String(exchange.getRequestBody().readAllBytes(), StandardCharsets.UTF_8);
            JsonNode composition;
            try {
                composition = Json.MAPPER.readTree(body);
            } catch (Exception e) {
                JsonNode err = Json.MAPPER.createObjectNode()
                    .put("success", false);
                respond(exchange, 400, err);
                return;
            }
            TranslationResult<JsonNode> result = SERVICE.openEhrToFhir(composition);
            respond(exchange, result.success() ? 200 : 400, Responses.envelope(result));
        });

        server.setExecutor(null);
        System.out.println("FHIR-OpenEHR-Bridge (Java) listening on :" + port);
        server.start();
    }

    private static void respond(HttpExchange exchange, int status, JsonNode body) throws IOException {
        byte[] payload = Json.MAPPER.writeValueAsBytes(body);
        exchange.getResponseHeaders().add("Content-Type", "application/json");
        exchange.sendResponseHeaders(status, payload.length);
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(payload);
        }
    }
}
