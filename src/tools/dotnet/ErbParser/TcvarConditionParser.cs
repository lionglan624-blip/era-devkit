namespace ErbParser;

public class TcvarConditionParser
{
    private readonly VariableConditionParser<TcvarRef> _parser = new("TCVAR");
    public TcvarRef? ParseTcvarCondition(string condition) => _parser.Parse(condition);
}
