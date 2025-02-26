using LoxGenerator;

namespace Lox;

public class Parser
{
    private class ParseError : Exception
    {
        public ParseError(Token token, string message) : base(message) { }
    }
    
    private List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens) => _tokens = tokens;
    

    private Expr Expression()
    {
        return Assigment();
    }

    private Expr Assigment()
    {
       var expr = Equality();

       if (LoxMatch(TokenType.Equal))
       {
           Token equals = Previous();
           Expr value = Assigment();

           if (expr is Expr.Variable variable)
           {
               Token name = ((Expr.Variable)expr).Name;
               return new Expr.Assign(name, value);
           }
           
           throw Error(equals, "Invalid assignment target.");
       }

       return expr;
    }

    private Expr Equality()
    {
        Expr expr = LoxComparison();

        while (LoxMatch(TokenType.BangEqual, TokenType.EqualEqual))
        {
            Token loxOperator = Previous();
            Expr right = LoxComparison();
            expr = new Expr.Binary(expr, loxOperator, right);
        }
        
        return expr;
    }
    
    private bool LoxMatch(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }
    
    private Token Advance() {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() =>  Peek().Type == TokenType.Eof;
    
    private Token Peek() => _tokens[_current];
    
    private Token Previous() => _tokens[_current -1];

    private Expr LoxComparison()
    {
        Expr expr = Term();

        while (LoxMatch(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)) {
            Token loxOperator = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, loxOperator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (LoxMatch(TokenType.Minus, TokenType.Plus))
        {
            Token loxOperator = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, loxOperator, right);
        }
        
        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (LoxMatch(TokenType.Slash, TokenType.Star)) {
            Token loxOperator = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, loxOperator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (LoxMatch(TokenType.Bang, TokenType.Minus)) {
            Token loxOperator = Previous();
            Expr right = Unary();
            return new Expr.Unary(loxOperator, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (LoxMatch(TokenType.False)) return new Expr.Literal(false);
        if (LoxMatch(TokenType.True)) return new Expr.Literal(true);
        if (LoxMatch(TokenType.Nil)) return new Expr.Literal(null);
        if (LoxMatch(TokenType.Number, TokenType.String)) {
            return new Expr.Literal(Previous().Literal);
        }

        if (LoxMatch(TokenType.Identifier))
        {
            return new Expr.Variable(Previous());
        }
        if (LoxMatch(TokenType.LeftParen)) {
            Expr expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        
        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        
        throw Error(Peek(), message);
    }

    private ParseError Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseError(token, message);
    }
    
    private void Synchronize() {
        Advance();

        while (!IsAtEnd()) {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type) {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }
    
    public List<Stmt> Parse() {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declarition());
        }

        return statements;
    }

    private Stmt Declarition()
    {
        try
        {
            if (LoxMatch(TokenType.Var)) return VarDeclaration();

            return Statements();
        }
        catch (ParseError exception)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(TokenType.Identifier, "Expect variable name.");
        
        Expr initializer = null;
        if (LoxMatch(TokenType.Equal))
        {
            initializer = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statements()
    {
        if (LoxMatch(TokenType.Print))
        {
            return PrintStatement();
        }

        return ExpressionStatement();
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Stmt.Print(value);
    }
}