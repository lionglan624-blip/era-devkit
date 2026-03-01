namespace ErbParser;

public class ItemConditionParser
{
    private readonly VariableConditionParser<ItemRef> _parser = new("ITEM");
    public ItemRef? ParseItemCondition(string condition) => _parser.Parse(condition);
}
