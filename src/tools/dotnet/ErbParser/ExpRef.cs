using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a EXP condition
/// Pattern: EXP:(target:)?(name|index)( op value)?
/// </summary>
public class ExpRef : VariableRef
{
}
