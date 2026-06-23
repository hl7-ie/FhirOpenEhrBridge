package io.github.hl7ie.fhiropenehrbridge;

import java.util.List;

/** Outcome of a translation in either direction. */
public record TranslationResult<T>(boolean success, T value, List<ValidationIssue> issues) {

    public static <T> TranslationResult<T> ok(T value, List<ValidationIssue> issues) {
        return new TranslationResult<>(true, value, issues);
    }

    public static <T> TranslationResult<T> fail(List<ValidationIssue> issues) {
        return new TranslationResult<>(false, null, issues);
    }
}
