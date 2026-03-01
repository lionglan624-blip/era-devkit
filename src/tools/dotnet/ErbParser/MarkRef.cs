using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a MARK condition
/// Pattern: MARK:(target:)?(name|index)( op value)?
/// </summary>
public class MarkRef : VariableRef
{
}
