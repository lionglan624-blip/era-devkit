namespace ErbParser;

public class TequipConditionParser
{
    private readonly VariableConditionParser<TequipRef> _parser = new("TEQUIP");
    public TequipRef? ParseTequipCondition(string condition) => _parser.Parse(condition);
}
