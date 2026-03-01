namespace ErbParser;

public class ExpConditionParser
{
    private readonly VariableConditionParser<ExpRef> _parser = new("EXP");
    public ExpRef? ParseExpCondition(string condition) => _parser.Parse(condition);
}
