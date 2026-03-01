namespace ErbParser;

public class TflagConditionParser
{
    private readonly VariableConditionParser<TflagRef> _parser = new("TFLAG");
    public TflagRef? ParseTflagCondition(string condition) => _parser.Parse(condition);
}
