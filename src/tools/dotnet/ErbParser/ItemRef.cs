using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to an ITEM condition
/// Pattern: ITEM:(target:)?(name|index)( op value)?
/// </summary>
public class ItemRef : VariableRef
{
}
