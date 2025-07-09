namespace EF.Blockchain.Domain;

/// <summary>
/// Represents the result of a validation process, indicating success or failure with an optional message.
/// </summary>
public class Validation
{
    /// <summary>
    /// Indicates whether the validation was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Message describing the validation result, usually used in case of failure.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Validation"/> class.
    /// </summary>
    /// <param name="success">Indicates if the validation passed (default: true).</param>
    /// <param name="message">Optional message explaining the validation result.</param>
    public Validation(bool success = true, string message = "")
    {
        Success = success;
        Message = message;
    }
}
