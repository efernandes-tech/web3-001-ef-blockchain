namespace EF.Blockchain.Domain;

/// <summary>
/// Validation class
/// </summary>
public class Validation
{
    public bool Success { get; }
    public string Message { get; }

    /// <summary>
    /// Creates a new validation object
    /// </summary>
    /// <param name="success">If the validation was successful</param>
    /// <param name="message">The validation message</param>
    public Validation(bool success = true, string message = "")
    {
        Success = success;
        Message = message;
    }
}
