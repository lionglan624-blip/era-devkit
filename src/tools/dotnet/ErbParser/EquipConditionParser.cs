namespace ErbParser;

public class EquipConditionParser
{
    private readonly VariableConditionParser<EquipRef> _parser = new("EQUIP");
    public EquipRef? ParseEquipCondition(string condition) => _parser.Parse(condition);
}
