using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Base class for variable reference conditions (CFLAG, TCVAR, EQUIP, ITEM, STAIN, MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM)
/// Note: TalentRef is NOT included (non-nullable Target/Name, no Index)
/// </summary>
public abstract class VariableRef : ICondition
{
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
