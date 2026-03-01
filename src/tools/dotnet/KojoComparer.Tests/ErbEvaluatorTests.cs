using Era.Core.Dialogue;
using Xunit;

namespace KojoComparer.Tests;

/// <summary>
/// Unit tests for ErbEvaluator and ExecutionContext.
/// Tests ERB AST execution: PRINTFORM, IF/ELSEIF/ELSE, PRINTDATA, SELECTCASE, assignments.
/// </summary>
public class ErbEvaluatorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ErbEvaluator _evaluator;

    public ErbEvaluatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        // Use empty gamePath - CSV loading is silently skipped for missing files
        _evaluator = new ErbEvaluator(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -------------------------------------------------------------------------
    // PRINTFORM basic execution
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintform_ReturnsContent()
    {
        // Arrange
        var erbContent = @"@KOJO_TEST_FUNC
PRINTFORM テスト台詞
";
        var path = WriteErb("test1.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_TEST_FUNC", state);

        // Assert
        Assert.Contains("テスト台詞", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintforml_ReturnsContentWithNewline()
    {
        // Arrange
        var erbContent = @"@KOJO_TEST_PRINTFORML
PRINTFORML 改行あり台詞
";
        var path = WriteErb("test_nl.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_TEST_PRINTFORML", state);

        // Assert
        Assert.Contains("改行あり台詞", output);
        Assert.Contains(DisplayMode.Newline, displayModes);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintformw_ReturnsWaitDisplayMode()
    {
        // Arrange
        var erbContent = @"@KOJO_TEST_PRINTFORMW
PRINTFORMW 待機台詞
";
        var path = WriteErb("test_w.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_TEST_PRINTFORMW", state);

        // Assert
        Assert.Contains("待機台詞", output);
        Assert.Contains(DisplayMode.Wait, displayModes);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithMultiplePrintforms_ReturnsAllContent()
    {
        // Arrange
        var erbContent = @"@KOJO_MULTI
PRINTFORM 行1
PRINTFORML 行2
";
        var path = WriteErb("test_multi.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_MULTI", state);

        // Assert
        Assert.Contains("行1", output);
        Assert.Contains("行2", output);
        Assert.Equal(2, displayModes.Count);
        Assert.Equal(DisplayMode.Default, displayModes[0]);
        Assert.Equal(DisplayMode.Newline, displayModes[1]);
    }

    // -------------------------------------------------------------------------
    // IF/ELSEIF/ELSE with TALENT state
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_IfBranchTrue_ExecutesIfBody()
    {
        // Arrange
        var erbContent = @"@KOJO_IF_TEST
IF TALENT:TARGET:16 != 0
    PRINTFORML 恋人台詞
ELSE
    PRINTFORML 通常台詞
ENDIF
";
        var path = WriteErb("test_if.ERB", erbContent);
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 1 } };

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_IF_TEST", state);

        // Assert
        Assert.Contains("恋人台詞", output);
        Assert.DoesNotContain("通常台詞", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_IfBranchFalse_ExecutesElseBody()
    {
        // Arrange
        var erbContent = @"@KOJO_IF_ELSE
IF TALENT:TARGET:16 != 0
    PRINTFORML 恋人台詞
ELSE
    PRINTFORML 通常台詞
ENDIF
";
        var path = WriteErb("test_ifelse.ERB", erbContent);
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 0 } };

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_IF_ELSE", state);

        // Assert
        Assert.DoesNotContain("恋人台詞", output);
        Assert.Contains("通常台詞", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_ElseIfBranchMatches_ExecutesElseIfBody()
    {
        // Arrange
        var erbContent = @"@KOJO_ELSEIF
IF TALENT:TARGET:16 != 0
    PRINTFORML 恋人台詞
ELSEIF TALENT:TARGET:17 != 0
    PRINTFORML 思慕台詞
ELSE
    PRINTFORML 通常台詞
ENDIF
";
        var path = WriteErb("test_elseif.ERB", erbContent);
        var state = new Dictionary<string, int>
        {
            { "TALENT:TARGET:16", 0 },
            { "TALENT:TARGET:17", 1 }
        };

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_ELSEIF", state);

        // Assert
        Assert.DoesNotContain("恋人台詞", output);
        Assert.Contains("思慕台詞", output);
        Assert.DoesNotContain("通常台詞", output);
    }

    // -------------------------------------------------------------------------
    // PRINTDATA / DATALIST / DATAFORM
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintDataBlock_ExecutesDataforms()
    {
        // Arrange
        var erbContent = @"@KOJO_PRINTDATA
PRINTDATA
    DATALIST
        DATAFORM データ行1
        DATAFORM データ行2
    ENDLIST
ENDDATA
";
        var path = WriteErb("test_printdata.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_PRINTDATA", state);

        // Assert
        Assert.Contains("データ行1", output);
        Assert.Contains("データ行2", output);
        Assert.Equal(2, displayModes.Count);
        Assert.All(displayModes, m => Assert.Equal(DisplayMode.Newline, m));
    }

    // -------------------------------------------------------------------------
    // RETURN node
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithReturn_StopsExecution()
    {
        // Arrange
        var erbContent = @"@KOJO_RETURN
PRINTFORML 最初の行
RETURN 0
PRINTFORML 二番目の行
";
        var path = WriteErb("test_return.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_RETURN", state);

        // Assert: RETURN stops execution so second PRINTFORML never executes
        Assert.Contains("最初の行", output);
        Assert.DoesNotContain("二番目の行", output);
    }

    // -------------------------------------------------------------------------
    // Function not found
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithNonExistentFunction_ThrowsInvalidOperationException()
    {
        // Arrange
        var erbContent = @"@DIFFERENT_FUNC
PRINTFORML 違う関数
";
        var path = WriteErb("test_notfound.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _evaluator.ExecuteAsync(path, "@KOJO_MISSING_FUNC", state));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_FunctionNameWithoutAtPrefix_FindsFunction()
    {
        // Arrange
        var erbContent = @"@KOJO_NO_AT
PRINTFORML アット無し
";
        var path = WriteErb("test_noat.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act - passing function name without @ prefix should still find it
        var (output, _) = await _evaluator.ExecuteAsync(path, "KOJO_NO_AT", state);

        // Assert
        Assert.Contains("アット無し", output);
    }

    // -------------------------------------------------------------------------
    // Assignment and SELECTCASE
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithNonLocalAssignment_TracksVariableInLocalDict()
    {
        // Arrange: assigns a non-LOCAL name then uses it in condition (e.g. 奴隷 = 5 pattern)
        // The assignment stores under key "奴隷" and "LOCAL:奴隷" in LocalVariables.
        // We verify the function runs without throwing and produces output.
        var erbContent = @"@KOJO_ASSIGNMENT
奴隷 = 5
PRINTFORML アサイン完了
";
        var path = WriteErb("test_assign.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_ASSIGNMENT", state);

        // Assert: execution ran without error
        Assert.Contains("アサイン完了", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithSelectCase_NoMatchFallsToDefault()
    {
        // Arrange: SELECTCASE with FLAG:0 (state value 0) - no case matches 0, falls to CASEELSE
        var erbContent = @"@KOJO_SELECTCASE
SELECTCASE FLAG:0
    CASE 1
        PRINTFORML ケース1
    CASE 2
        PRINTFORML ケース2
    CASEELSE
        PRINTFORML その他
ENDSELECT
";
        var path = WriteErb("test_select.ERB", erbContent);
        // FLAG:0 = 0 by default → no CASE matches → CASEELSE
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_SELECTCASE", state);

        // Assert: CASEELSE executed
        Assert.Contains("その他", output);
        Assert.DoesNotContain("ケース1", output);
        Assert.DoesNotContain("ケース2", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithSelectCase_MatchesExplicitStateValue()
    {
        // Arrange: SELECTCASE matching a FLAG value set in state
        var erbContent = @"@KOJO_SELECTCASE_MATCH
SELECTCASE FLAG:0
    CASE 1
        PRINTFORML ケース1
    CASE 2
        PRINTFORML ケース2
    CASEELSE
        PRINTFORML その他
ENDSELECT
";
        var path = WriteErb("test_select_match.ERB", erbContent);
        var state = new Dictionary<string, int> { { "FLAG:0", 2 } };

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_SELECTCASE_MATCH", state);

        // Assert: CASE 2 matched
        Assert.Contains("ケース2", output);
        Assert.DoesNotContain("ケース1", output);
        Assert.DoesNotContain("その他", output);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithSelectCaseElse_FallsToDefault()
    {
        // Arrange
        var erbContent = @"@KOJO_SELECTCASE_ELSE
LOCAL = 99
SELECTCASE LOCAL
    CASE 1
        PRINTFORML ケース1
    CASEELSE
        PRINTFORML その他
ENDSELECT
";
        var path = WriteErb("test_select_else.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_SELECTCASE_ELSE", state);

        // Assert
        Assert.Contains("その他", output);
        Assert.DoesNotContain("ケース1", output);
    }

    // -------------------------------------------------------------------------
    // CALL node - intra-file function calls
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithCallToIntraFileFunction_ExecutesCalledBody()
    {
        // Arrange: main function calls a helper function defined in the same file
        var erbContent = @"@KOJO_MAIN
CALL KOJO_HELPER
PRINTFORML メイン台詞

@KOJO_HELPER
PRINTFORML ヘルパー台詞
";
        var path = WriteErb("test_call.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (output, _) = await _evaluator.ExecuteAsync(path, "@KOJO_MAIN", state);

        // Assert: helper function output appears
        Assert.Contains("ヘルパー台詞", output);
        Assert.Contains("メイン台詞", output);
    }

    // -------------------------------------------------------------------------
    // ExecutionContext direct construction
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void ExecutionContext_DefaultState_HasEmptyOutput()
    {
        // Arrange
        var state = new Dictionary<string, int>();
        var talentMap = new Dictionary<string, int>();
        var ablMap = new Dictionary<string, int>();
        var cflagMap = new Dictionary<string, int>();

        // Act
        var ctx = new ExecutionContext(state, talentMap, ablMap, cflagMap);

        // Assert
        Assert.Equal(string.Empty, ctx.Output.ToString());
        Assert.Empty(ctx.DisplayModes);
        Assert.False(ctx.Returned);
        Assert.Empty(ctx.LocalVariables);
        Assert.Same(state, ctx.State);
        Assert.Same(talentMap, ctx.TalentNameToIndex);
        Assert.Same(ablMap, ctx.AblNameToIndex);
        Assert.Same(cflagMap, ctx.CflagNameToIndex);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ExecutionContext_SetReturned_CanBeRead()
    {
        // Arrange
        var ctx = new ExecutionContext(
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>());

        // Act
        ctx.Returned = true;

        // Assert
        Assert.True(ctx.Returned);
    }

    // -------------------------------------------------------------------------
    // CSV loading with temp files
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public void ErbEvaluator_WithMissingCsvFiles_ConstructsSuccessfully()
    {
        // Arrange - directory exists but no CSV files
        var emptyDir = Path.Combine(_tempDir, "empty_game");
        Directory.CreateDirectory(emptyDir);

        // Act - should not throw
        var evaluator = new ErbEvaluator(emptyDir);

        // Assert
        Assert.NotNull(evaluator);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ErbEvaluator_WithCsvMappings_UsesNameToIndexMapping()
    {
        // Arrange: create CSV file mapping talent names to indices
        var csvDir = Path.Combine(_tempDir, "csv_game", "CSV");
        Directory.CreateDirectory(csvDir);
        File.WriteAllText(Path.Combine(csvDir, "TALENT.csv"), "16,恋人,;lover\n17,思慕,;longing\n");
        File.WriteAllText(Path.Combine(csvDir, "ABL.csv"), "");
        File.WriteAllText(Path.Combine(csvDir, "CFLAG.csv"), "");

        var evaluatorWithCsv = new ErbEvaluator(Path.Combine(_tempDir, "csv_game"));

        // ERB using TALENT name (not index)
        var erbContent = @"@KOJO_CSV_NAME
IF TALENT:恋人
    PRINTFORML 恋人名前でマッチ
ELSE
    PRINTFORML マッチなし
ENDIF
";
        var path = WriteErb("test_csv.ERB", erbContent);

        // State uses target:index format matching the CSV-resolved index
        var state = new Dictionary<string, int> { { "TALENT:TARGET:16", 1 } };

        // Act
        var (output, _) = await evaluatorWithCsv.ExecuteAsync(path, "@KOJO_CSV_NAME", state);

        // Assert: talent name resolved to index 16 from CSV
        Assert.Contains("恋人名前でマッチ", output);
    }

    // -------------------------------------------------------------------------
    // DisplayMode variants
    // -------------------------------------------------------------------------

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintformk_ReturnsKeyWaitMode()
    {
        // Arrange
        var erbContent = @"@KOJO_KEYWAIT
PRINTFORMK キー待ち
";
        var path = WriteErb("test_k.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (_, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_KEYWAIT", state);

        // Assert
        Assert.Contains(DisplayMode.KeyWait, displayModes);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ExecuteAsync_WithPrintformkl_ReturnsKeyWaitNewlineMode()
    {
        // Arrange
        var erbContent = @"@KOJO_KEYWAITAL
PRINTFORMKL キー待ち改行
";
        var path = WriteErb("test_kl.ERB", erbContent);
        var state = new Dictionary<string, int>();

        // Act
        var (_, displayModes) = await _evaluator.ExecuteAsync(path, "@KOJO_KEYWAITAL", state);

        // Assert
        Assert.Contains(DisplayMode.KeyWaitNewline, displayModes);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string WriteErb(string fileName, string content)
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content, System.Text.Encoding.UTF8);
        return path;
    }
}
