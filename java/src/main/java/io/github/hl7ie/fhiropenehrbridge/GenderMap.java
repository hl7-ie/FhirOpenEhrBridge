package io.github.hl7ie.fhiropenehrbridge;

/** Gender mapping between FHIR administrative-gender codes and openEHR codes. */
public final class GenderMap {
    private GenderMap() {}

    public static String toOpenEhr(String fhirGender) {
        if (fhirGender == null) {
            return "unknown";
        }
        return switch (fhirGender.trim().toLowerCase()) {
            case "male" -> "male";
            case "female" -> "female";
            case "other" -> "intersex";
            default -> "unknown";
        };
    }

    /** Returns {@code null} for empty input (meaning "do not set gender"). */
    public static String toFhir(String openEhrGender) {
        if (openEhrGender == null) {
            return null;
        }
        return switch (openEhrGender.trim().toLowerCase()) {
            case "male" -> "male";
            case "female" -> "female";
            case "intersex" -> "other";
            case "unknown" -> "unknown";
            case "" -> null;
            default -> "unknown";
        };
    }
}
