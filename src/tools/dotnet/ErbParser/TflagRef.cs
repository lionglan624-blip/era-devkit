using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a TFLAG condition
/// Pattern: TFLAG:(target:)?(name|index)( op value)?
/// </summary>
public class TflagRef : VariableRef
{
}
