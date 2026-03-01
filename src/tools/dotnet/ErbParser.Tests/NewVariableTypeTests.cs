using ErbParser;
using Xunit;

namespace ErbParser.Tests;

public class NewVariableTypeTests
{
    [Fact]
    public void Parse_MarkCondition_WithTargetAndName()
    {
        // Parse "MARK:MASTER:100" → Target="MASTER", Index=100
        var parser = new MarkConditionParser();
        var result = parser.ParseMarkCondition("MARK:MASTER:100");
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal(100, result.Index);
    }

    [Fact]
    public void Parse_ExpCondition_WithTargetAndName()
    {
        // Parse "EXP:MASTER:才能" → Target="MASTER", Name="才能"
        var parser = new ExpConditionParser();
        var result = parser.ParseExpCondition("EXP:MASTER:才能");
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("才能", result.Name);
    }

    [Fact]
    public void Parse_NowexCondition_WithTargetAndName()
    {
        // Parse "NOWEX:TARGET:口説き" → Target="TARGET", Name="口説き"
        var parser = new NowexConditionParser();
        var result = parser.ParseNowexCondition("NOWEX:TARGET:口説き");
        Assert.NotNull(result);
        Assert.Equal("TARGET", result.Target);
        Assert.Equal("口説き", result.Name);
    }

    [Fact]
    public void Parse_AblCondition_WithTargetNameAndOperator()
    {
        // Parse "ABL:TARGET:知力 >= 500" → Target="TARGET", Name="知力", Operator=">=", Value="500"
        var parser = new AblConditionParser();
        var result = parser.ParseAblCondition("ABL:TARGET:知力 >= 500");
        Assert.NotNull(result);
        Assert.Equal("TARGET", result.Target);
        Assert.Equal("知力", result.Name);
        Assert.Equal(">=", result.Operator);
        Assert.Equal("500", result.Value);
    }

    [Fact]
    public void Parse_FlagCondition_WithNameAndOperator()
    {
        // Parse "FLAG:好感度パターン == 1" → Name="好感度パターン", Operator="==", Value="1"
        var parser = new FlagConditionParser();
        var result = parser.ParseFlagCondition("FLAG:好感度パターン == 1");
        Assert.NotNull(result);
        Assert.Equal("好感度パターン", result.Name);
        Assert.Equal("==", result.Operator);
        Assert.Equal("1", result.Value);
    }

    [Fact]
    public void Parse_TflagCondition_WithName()
    {
        // Parse "TFLAG:コマンド成功度" → Name="コマンド成功度"
        var parser = new TflagConditionParser();
        var result = parser.ParseTflagCondition("TFLAG:コマンド成功度");
        Assert.NotNull(result);
        Assert.Equal("コマンド成功度", result.Name);
    }

    [Fact]
    public void Parse_TequipCondition_WithTargetAndName()
    {
        // Parse "TEQUIP:TARGET:下半身上着１" → Target="TARGET", Name="下半身上着１"
        var parser = new TequipConditionParser();
        var result = parser.ParseTequipCondition("TEQUIP:TARGET:下半身上着１");
        Assert.NotNull(result);
        Assert.Equal("TARGET", result.Target);
        Assert.Equal("下半身上着１", result.Name);
    }

    [Fact]
    public void Parse_PalamCondition_WithTargetNameAndOperator()
    {
        // Parse "PALAM:MASTER:潤滑 > PALAMLV:3" → Target="MASTER", Name="潤滑", Operator=">", Value="PALAMLV:3"
        // Note: Value is a non-numeric string "PALAMLV:3" (C5 constraint - non-numeric comparison values)
        var parser = new PalamConditionParser();
        var result = parser.ParsePalamCondition("PALAM:MASTER:潤滑 > PALAMLV:3");
        Assert.NotNull(result);
        Assert.Equal("MASTER", result.Target);
        Assert.Equal("潤滑", result.Name);
        Assert.Equal(">", result.Operator);
        Assert.Equal("PALAMLV:3", result.Value);
    }

    [Fact]
    public void Parse_WrongPrefix_ReturnsNull()
    {
        // Use MarkConditionParser to try parsing "INVALID:x" → result is null
        var parser = new MarkConditionParser();
        var result = parser.ParseMarkCondition("INVALID:x");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_NonNumericValue_TflagConstName()
    {
        // Parse "TFLAG:コマンド成功度 == 成功度_失敗" → Name="コマンド成功度", Operator="==", Value="成功度_失敗"
        var parser = new TflagConditionParser();
        var result = parser.ParseTflagCondition("TFLAG:コマンド成功度 == 成功度_失敗");
        Assert.NotNull(result);
        Assert.Equal("コマンド成功度", result.Name);
        Assert.Equal("==", result.Operator);
        Assert.Equal("成功度_失敗", result.Value);
    }
}
