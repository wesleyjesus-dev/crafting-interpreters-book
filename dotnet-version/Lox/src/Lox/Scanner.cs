namespace Lox;

using LoxGenerator;
public class Scanner(string source)
{
    private string _source = source;
    private List<Token> _tokens = new();
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private static Dictionary<string, TokenType> keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While }
    };

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        
        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }
    
    private void ScanToken()
    {
        char c = Advanced();
        switch (c)  
        {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break;
            case '!': AddToken(MatchWith('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=': AddToken(MatchWith('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<': AddToken(MatchWith('=') ? TokenType.LessEqual : TokenType.Less); break;
            case '>': AddToken(MatchWith('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
            case '/':
                if (MatchWith('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advanced();
                    }

                }
                else if (MatchWith('*'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advanced();

                        if (IsEndFromCommend())
                        {
                            break;
                        }
                    }
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace
                break;

            case '\n':
                _line++;
                break;
            
            case '"': AsString(); break;
            case 'o':
                if (MatchWith('r'))
                {
                    AddToken(TokenType.Or);
                }
                break;

            default:
                if (IsDigit(c))
                {
                    AsNumber();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Program.Error(_line, "Unexpected character.");
                }
                break;
        }
    }

    public bool IsEndFromCommend()
    {
        if (_source[_current] == '*' && PeekNext() == '/')
        {
            _current++;
            return true;
        }
        return false;
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            Advanced();
        }
        
        var text = source.JSubstring(_start, _current);
        if (!keywords.TryGetValue(text, out TokenType keyword))
        {
            keyword = TokenType.Identifier;
        }

        AddToken(keyword);
    }

    private bool IsAlpha(char c) => char.IsLetter(c) || c == '_';
    
    private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    
    /// <summary>
    /// Create a token from Number
    /// </summary>
    public void AsNumber()
    {
        while (IsDigit(Peek()))
        {
            Advanced();
        }
        
        if ((Peek() == '.') && IsDigit(PeekNext()))
        {
            Advanced();

            while (IsDigit(Peek()))
            {
                Advanced();
            }
        }
        
        AddToken(TokenType.Number, double.Parse(_source.Substring(_start, _current)));
    }

    /// <summary>
    /// Verify if contains more digit before add new token
    /// </summary>
    /// <returns></returns>
    public char PeekNext()
    {
        if (_current + 1 >= _source.Length)
        {
            return '\0';
        }
        return _source[_current + 1];
    }

    /// <summary>
    ///  Verify if 'c' is digit
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool IsDigit(char c) => char.IsDigit(c); // c >= '0' && c <= '9';
    
    
    /// <summary>
    /// Get string from source
    /// </summary>
    public void AsString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++; 
            }

            Advanced();
        }

        if (IsAtEnd())
        {
            Program.Error(_line, "Unterminated string.");
            return;
        }

        // The closing ".
        Advanced();

        var value = _source.JSubstring(_start + 1, _current - 1);
        AddToken(TokenType.String, value);
    }
    
    /// <summary>
    /// Verify if is end of file EOF
    /// this '\0' is used for say when is end of file
    /// </summary>
    /// <returns></returns>
    public char Peek()
    {
        if(IsAtEnd()) return '\0'; 
        return _source[_current];
    }
    
    /// <summary>
    /// This method verify if current operator is containing another symbol, like != or <> or == and etc..
    /// </summary>
    /// <param name="expected"></param>
    /// <returns></returns>
    private bool MatchWith(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }
    
    /// <summary>
    /// Get next character in the source file
    /// </summary>
    /// <returns></returns>
    private char Advanced() => _source[_current++];
    private void AddToken(TokenType type) => AddToken(type, null);
    private void AddToken(TokenType type, object literal)
    {
        var text = _source.JSubstring(_start, _current);
        var token = new Token(type, text, literal, _line);
        _tokens.Add(token);
    }

    private bool IsAtEnd() => _current >= _source.Length;
}