namespace LoxGenerator;

/// <summary>
/// Represents a static class containing definitions for tokens and token types
/// used in the generation of a programming language lexical analyzer.
///
/// This class is required because Expr and Visitors depends directly
/// from Token class e by this was necessary create this class on this LoxGenerator
/// </summary>
public static class TokenClasses
{
    public const string TokenEnum = @"
public enum TokenType
{
    // Single-character tokens.
    LeftParen, 
    RightParen, 
    LeftBrace, 
    RightBrace,
    Comma, 
    Dot, 
    Minus, 
    Plus, 
    Semicolon, 
    Slash, 
    Star,

    // One or two character tokens.
    Bang, // ! 
    BangEqual, // !=
    Equal, // =
    EqualEqual, // ==
    Greater, // >
    GreaterEqual, // >=
    Less, // <
    LessEqual, // <=

    // Literals.
    Identifier, 
    String, 
    Number,

    // Keywords.
    And, 
    Class, 
    Else, 
    False, 
    Fun, 
    For, 
    If, 
    Nil, 
    Or,
    Print, 
    Return, 
    Super, 
    This, 
    True, 
    Var, 
    While,

    Eof
}
";
    
    public const string Token = @"
public class Token
{
    public Token(TokenType type, string lexeme, object literal, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public TokenType Type { get; set; }
    public string Lexeme { get; set; }
    public Object Literal { get; set; }
    public int Line { get; set; }

    public override string ToString() => $""{Type} {Lexeme} {Literal}"";
}
";
}