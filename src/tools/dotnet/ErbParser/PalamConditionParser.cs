namespace ErbParser;

public class PalamConditionParser
{
    private readonly VariableConditionParser<PalamRef> _parser = new("PALAM");
    public PalamRef? ParsePalamCondition(string condition) => _parser.Parse(condition);
}
