using System.Text.Json;
using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class ArgRefSerializationTests
{
    [Fact]
    public void Serialize_ArgRef_ContainsTypeDiscriminator()
    {
        ICondition condition = new ArgRef { Index = 0, Operator = "==", Value = "2" };
        var json = JsonSerializer.Serialize(condition);
        Assert.Contains("\"$type\":\"arg\"", json);
    }

    [Fact]
    public void Deserialize_ArgRefJson_ReturnsCorrectProperties()
    {
        var json = "{\"$type\":\"arg\",\"index\":1,\"operator\":\"==\",\"value\":\"3\"}";
        var result = JsonSerializer.Deserialize<ICondition>(json);
        var argRef = Assert.IsType<ArgRef>(result);
        Assert.Equal(1, argRef.Index);
        Assert.Equal("==", argRef.Operator);
        Assert.Equal("3", argRef.Value);
    }

    [Fact]
    public void Polymorphic_SerializeAsICondition_WorksCorrectly()
    {
        ICondition condition = new ArgRef { Index = 0, Operator = null, Value = null };
        var json = JsonSerializer.Serialize(condition);
        var deserialized = JsonSerializer.Deserialize<ICondition>(json);
        var argRef = Assert.IsType<ArgRef>(deserialized);
        Assert.Equal(0, argRef.Index);
        Assert.Null(argRef.Operator);
        Assert.Null(argRef.Value);
    }
}
