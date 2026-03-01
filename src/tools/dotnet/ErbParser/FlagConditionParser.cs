namespace ErbParser;

public class FlagConditionParser
{
    private readonly VariableConditionParser<FlagRef> _parser = new("FLAG");
    public FlagRef? ParseFlagCondition(string condition) => _parser.Parse(condition);
}
