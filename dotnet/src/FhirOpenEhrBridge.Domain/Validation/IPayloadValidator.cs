namespace FhirOpenEhrBridge.Domain.Validation;

/// <summary>
/// Validates an incoming payload <em>before</em> it is handed to a mapper. The
/// translation pipeline aborts when validation produces any
/// <see cref="ValidationSeverity.Error"/> issue.
/// </summary>
/// <typeparam name="T">The payload type being validated (e.g. a JSON string or a model).</typeparam>
public interface IPayloadValidator<in T>
{
    /// <summary>Validates the supplied payload.</summary>
    /// <param name="payload">The payload to inspect.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any issues found.</returns>
    ValidationResult Validate(T payload);
}
