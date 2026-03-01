using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a CFLAG condition
/// Pattern: CFLAG:(target:)?(name|index)( op value)?
/// </summary>
public class CflagRef : VariableRef
{
}
