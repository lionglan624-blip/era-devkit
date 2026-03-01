namespace ErbParser;

public class CflagConditionParser
{
    private readonly VariableConditionParser<CflagRef> _parser = new("CFLAG");
    public CflagRef? ParseCflagCondition(string condition) => _parser.Parse(condition);
}
