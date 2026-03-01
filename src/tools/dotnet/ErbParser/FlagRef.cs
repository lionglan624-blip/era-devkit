using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a FLAG condition
/// Pattern: FLAG:(target:)?(name|index)( op value)?
/// </summary>
public class FlagRef : VariableRef
{
}
