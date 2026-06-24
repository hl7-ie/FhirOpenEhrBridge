package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.databind.ObjectMapper;

/** Shared Jackson {@link ObjectMapper} (thread-safe, reusable). */
public final class Json {
    private Json() {}

    public static final ObjectMapper MAPPER = new ObjectMapper();
}
