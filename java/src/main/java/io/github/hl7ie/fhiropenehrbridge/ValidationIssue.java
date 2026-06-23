package io.github.hl7ie.fhiropenehrbridge;

import com.fasterxml.jackson.annotation.JsonInclude;

/** A single problem found while validating a payload. */
@JsonInclude(JsonInclude.Include.NON_NULL)
public record ValidationIssue(String severity, String message, String location) {

    public static ValidationIssue error(String message, String location) {
        return new ValidationIssue("error", message, location);
    }

    public static ValidationIssue warning(String message, String location) {
        return new ValidationIssue("warning", message, location);
    }
}
