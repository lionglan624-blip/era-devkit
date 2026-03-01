using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Marker interface for all condition types in ERB conditional expressions.
/// Enables polymorphic handling of TALENT, CFLAG, function calls, and logical operations.
/// </summary>
[JsonDerivedType(typeof(TalentRef), typeDiscriminator: "talent")]
[JsonDerivedType(typeof(CflagRef), typeDiscriminator: "cflag")]
[JsonDerivedType(typeof(TcvarRef), typeDiscriminator: "tcvar")]
[JsonDerivedType(typeof(NegatedCondition), typeDiscriminator: "negated")]
[JsonDerivedType(typeof(FunctionCall), typeDiscriminator: "function")]
[JsonDerivedType(typeof(LogicalOp), typeDiscriminator: "logical")]
[JsonDerivedType(typeof(EquipRef), typeDiscriminator: "equip")]
[JsonDerivedType(typeof(ItemRef), typeDiscriminator: "item")]
[JsonDerivedType(typeof(StainRef), typeDiscriminator: "stain")]
[JsonDerivedType(typeof(MarkRef), typeDiscriminator: "mark")]
[JsonDerivedType(typeof(ExpRef), typeDiscriminator: "exp")]
[JsonDerivedType(typeof(NowexRef), typeDiscriminator: "nowex")]
[JsonDerivedType(typeof(AblRef), typeDiscriminator: "abl")]
[JsonDerivedType(typeof(FlagRef), typeDiscriminator: "flag")]
[JsonDerivedType(typeof(TflagRef), typeDiscriminator: "tflag")]
[JsonDerivedType(typeof(TequipRef), typeDiscriminator: "tequip")]
[JsonDerivedType(typeof(PalamRef), typeDiscriminator: "palam")]
[JsonDerivedType(typeof(ArgRef), typeDiscriminator: "arg")]
[JsonDerivedType(typeof(LocalRef), typeDiscriminator: "local")]
[JsonDerivedType(typeof(BitwiseComparisonCondition), typeDiscriminator: "bitwise_comparison")]
public interface ICondition
{
}
