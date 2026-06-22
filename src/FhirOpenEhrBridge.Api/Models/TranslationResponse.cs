using FhirOpenEhrBridge.Domain.Validation;

namespace FhirOpenEhrBridge.Api.Models;

/// <summary>
/// Envelope returned by the translation endpoints, carrying the mapped payload
/// alongside any non-fatal validation issues.
/// </summary>
/// <typeparam name="T">The type of the mapped result payload.</typeparam>
/// <param name="Success">Whether the translation produced a usable <paramref name="Result"/>.</param>
/// <param name="Result">The mapped payload, or <c>null</c> on failure.</param>
/// <param name="Issues">Validation issues (errors on failure, warnings/info on success).</param>
public sealed record TranslationResponse<T>(
    bool Success,
    T? Result,
    IReadOnlyList<ValidationIssue> Issues);
