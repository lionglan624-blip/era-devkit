namespace ErbParser;

public class NowexConditionParser
{
    private readonly VariableConditionParser<NowexRef> _parser = new("NOWEX");
    public NowexRef? ParseNowexCondition(string condition) => _parser.Parse(condition);
}
