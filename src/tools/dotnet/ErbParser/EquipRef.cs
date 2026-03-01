using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to an EQUIP condition
/// Pattern: EQUIP:(target:)?(name|index)( op value)?
/// </summary>
public class EquipRef : VariableRef
{
}
