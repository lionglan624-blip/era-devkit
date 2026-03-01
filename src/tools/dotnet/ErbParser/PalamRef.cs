using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a PALAM condition
/// Pattern: PALAM:(target:)?(name|index)( op value)?
/// </summary>
public class PalamRef : VariableRef
{
}
