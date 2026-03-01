using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a TEQUIP condition
/// Pattern: TEQUIP:(target:)?(name|index)( op value)?
/// </summary>
public class TequipRef : VariableRef
{
}
