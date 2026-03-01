using System;

namespace ErbToYaml;

/// <summary>
/// Exception thrown when YAML validation against schema fails
/// Feature 361 - AC#2 implementation
/// </summary>
public class SchemaValidationException : Exception
{
    public SchemaValidationException(string message) : base(message)
    {
    }

    public SchemaValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
