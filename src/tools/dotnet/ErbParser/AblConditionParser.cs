namespace ErbParser;

public class AblConditionParser
{
    private readonly VariableConditionParser<AblRef> _parser = new("ABL");
    public AblRef? ParseAblCondition(string condition) => _parser.Parse(condition);
}
