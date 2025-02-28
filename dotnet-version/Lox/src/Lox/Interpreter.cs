using LoxGenerator;

namespace Lox;

public struct Unit
{
    public static Unit Value => default;
}
public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Unit>
{
    private Environment _enviroment = new Environment();
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError exception)
        {
            Program.RuntimeError(exception);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    private string Stringify(object value)
    {
        if (value is null) return "nil";
        if (value is double)
        {
            var text = value.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.JSubstring(0, text.Length - 2);
            }
            return text;
        }
        return value.ToString();
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        _enviroment.Assign(expr.Name, value);
        return value;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Opt.Type)
        {
            case TokenType.Greater:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left <= (double)right;
            case TokenType.Minus:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left - (double)right;
            case TokenType.Plus:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }

                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }

                if (left is string && right is double)
                {
                    return (string)left + right;
                }

                if (left is double && right is string)
                {
                    return left + (string)right;
                }

                throw new RuntimeError(expr.Opt, "Operands must be two numbers or two strings.");
            case TokenType.Slash:
                CheckNumberOperands(expr.Opt, left, right);
                CheckDivisorByZero(expr.Opt, right);
                return (double)left / (double)right;
            case TokenType.Star:
                CheckNumberOperands(expr.Opt, left, right);
                return (double)left * (double)right;
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return !IsEqual(left, right);
        }

        //  unrechable.
        return null;
    }
    private void CheckDivisorByZero(Token exprOpt, object right)
    {
        if ((double)right == 0)
            throw new RuntimeError(exprOpt, "Cannot divide by zero.");
    }
    private void CheckNumberOperand(Token token, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(token, "Operand must be a number.");
    }
    private void CheckNumberOperands(Token token, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(token, "Operands must be numbers.");
    }
    private bool IsEqual(object left, object right)
    {
        if (left is null && right is null) return true;
        if (left is null) return false;
        return left.Equals(right);
    }
    
    /// <summary>
    /// Visits a grouping expression node in the Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="expr">The grouping expression to evaluate, which contains an inner expression enclosed in parentheses.</param>
    /// <returns>
    /// The evaluated result of the inner expression within the grouping construct.
    /// </returns>
    public object VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expression);

    /// <summary>
    /// Evaluates the given expression node in the Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="exprExpression">The expression to evaluate, which may represent a literal, grouping, binary, or unary operation.</param>
    /// <returns>
    /// The result of evaluating the expression, which can vary based on the type of the node.
    /// </returns>
    private object Evaluate(Expr exprExpression) => exprExpression.Accept(this);

    /// <summary>
    /// Visits a literal expression node in the Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="expr">The literal expression to evaluate, which represents a constant value such as a number, string, or boolean.</param>
    /// <returns>
    /// The constant value represented by the literal expression.
    /// </returns>
    public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;

    /// <summary>
    /// Visits a unary expression node in the Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="expr">The unary expression to evaluate, which includes an operator and a right-hand operand.</param>
    /// <returns>
    /// The evaluated result of the unary operation, which can vary depending on the operator type.
    /// </returns>
    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Opt.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(right);
            case TokenType.Minus:
                CheckNumberOperand(expr.Opt, right);
                return -(double)right;
        }
        
        //  unrechable.
        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr) => _enviroment.Get(expr.Name);
    
    private bool IsTruthy(object rightExpr)
    {
        if (rightExpr is null) return false;
        if (rightExpr is bool b) return b;
        return true;
    }

    public class RuntimeError: Exception
    {
        public RuntimeError(Token token, string message)
        : base($"Token: {token} | Message: {message}")
        {
            Token = token;
        }
        public Token Token;
    }

    public Unit VisitBlockStmt(Stmt.Block expr)
    {
        ExecuteBlock(expr.Statements, new Environment(_enviroment));
        return Unit.Value;
    }

    private void ExecuteBlock(List<Stmt> exprStatements, Environment environment)
    {
        var previous = _enviroment;
        try
        {
            _enviroment = environment;
            foreach (var statement in exprStatements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _enviroment = previous;
        }
    }

    public Unit VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.LoxExpression);
        return Unit.Value;
    }

    public Unit VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.LoxExpression);
        Console.WriteLine(Stringify(value));
        return Unit.Value;
    }

    public Unit VisitVarStmt(Stmt.Var stmt)
    {
        object value = null;
        if (stmt.Initializer != null) {
            value = Evaluate(stmt.Initializer);
        }

        _enviroment.Define(stmt.Name.Lexeme, value);
        return Unit.Value;
    }
}