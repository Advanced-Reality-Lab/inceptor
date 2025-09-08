namespace InceptorEngine.Editor.Validation
{
    /// <summary>
    /// Represents the severity of a validation issue.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// A critical configuration error that will likely cause a runtime exception.
        /// </summary>
        Error,
        /// <summary>
        /// A potential issue or missing best practice that may not break the system but is worth noting.
        /// </summary>
        Warning
    }

    /// <summary>
    /// A data structure that holds the result of a single validation check.
    /// </summary>
    public struct ValidationResult
    {
        public ValidationSeverity Severity;
        public string Message;
        public UnityEngine.Object ContextObject; // Allows us to ping the problematic asset in the editor.

        public ValidationResult(ValidationSeverity severity, string message, UnityEngine.Object contextObject = null)
        {
            Severity = severity;
            Message = message;
            ContextObject = contextObject;
        }
    }
}
