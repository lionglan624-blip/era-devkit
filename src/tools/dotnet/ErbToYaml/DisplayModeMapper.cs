namespace ErbToYaml;

public static class DisplayModeMapper
{
    public static string? MapVariant(string variant)
    {
        return variant switch
        {
            "PRINTDATA" => null,
            "PRINTDATAL" => "newline",
            "PRINTDATAW" => "wait",
            "PRINTDATAK" => "keyWait",
            "PRINTDATAKL" => "keyWaitNewline",
            "PRINTDATAKW" => "keyWaitWait",
            "PRINTDATAD" => "display",
            "PRINTDATADL" => "displayNewline",
            "PRINTDATADW" => "displayWait",
            "PRINTFORML" => "newline",
            "PRINTFORMW" => "wait",
            "PRINTFORMK" => "keyWait",
            "PRINTFORM" => null,
            _ => null
        };
    }
}
