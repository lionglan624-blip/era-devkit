namespace ErbLinter.Parser;

/// <summary>
/// Token types for ERB script lexical analysis
/// </summary>
public enum TokenType
{
    // Literals
    Number,         // 123, 0x1F
    String,         // "hello"
    Identifier,     // variable names, function names

    // Keywords (subset - most common)
    If,
    Else,
    ElseIf,
    EndIf,
    Sif,
    For,
    Next,
    While,
    Wend,
    Repeat,
    Rend,
    Do,
    Loop,
    SelectCase,
    Case,
    CaseElse,
    EndSelect,
    Call,
    TryCall,
    Jump,
    Goto,
    Return,
    Print,
    PrintL,
    PrintForm,
    PrintFormL,
    PrintData,
    EndData,
    Data,
    DataForm,

    // Operators
    Plus,           // +
    Minus,          // -
    Multiply,       // *
    Divide,         // /
    Modulo,         // %
    Assign,         // =
    Equal,          // ==
    NotEqual,       // !=
    Less,           // <
    LessEqual,      // <=
    Greater,        // >
    GreaterEqual,   // >=
    And,            // &&
    Or,             // ||
    Not,            // !
    BitAnd,         // &
    BitOr,          // |

    // Punctuation
    OpenParen,      // (
    CloseParen,     // )
    OpenBrace,      // {
    CloseBrace,     // }
    OpenBracket,    // [
    CloseBracket,   // ]
    Comma,          // ,
    Colon,          // :
    Semicolon,      // ; (comment start)
    At,             // @ (function definition)
    Hash,           // # (directive)
    Percent,        // % (interpolation)

    // Special
    Comment,        // ; comment text
    Whitespace,     // spaces, tabs
    Newline,        // end of line
    EndOfFile,      // end of input
    Unknown         // unrecognized
}

/// <summary>
/// A single token from ERB source
/// </summary>
public record Token(
    TokenType Type,
    string Value,
    int Line,
    int Column,
    int Length);
