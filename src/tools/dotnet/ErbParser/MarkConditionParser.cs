namespace ErbParser;

public class MarkConditionParser
{
    private readonly VariableConditionParser<MarkRef> _parser = new("MARK");
    public MarkRef? ParseMarkCondition(string condition) => _parser.Parse(condition);
}
