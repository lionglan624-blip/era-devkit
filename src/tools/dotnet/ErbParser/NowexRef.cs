using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a NOWEX condition
/// Pattern: NOWEX:(target:)?(name|index)( op value)?
/// </summary>
public class NowexRef : VariableRef
{
}
