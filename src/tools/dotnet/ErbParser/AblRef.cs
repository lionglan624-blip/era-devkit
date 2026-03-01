using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a ABL condition
/// Pattern: ABL:(target:)?(name|index)( op value)?
/// </summary>
public class AblRef : VariableRef
{
}
