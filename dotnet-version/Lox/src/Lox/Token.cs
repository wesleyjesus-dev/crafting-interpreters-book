namespace Lox;
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

    public override string ToString() => $"{Type} {Lexeme} {Literal}";
}