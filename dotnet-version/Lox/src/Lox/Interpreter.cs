using LoxGenerator;

namespace Lox;

public class Interpreter : Expr.IVisitor<object>
{
    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Opt.Type)
        {
            case TokenType.Greater:
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                return (double)left >= (double)right;
            case TokenType.Less:
                return (double)left < (double)right;
            case TokenType.LessEqual:
                return (double)left <= (double)right;
            case TokenType.Minus:
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
                break;
            case TokenType.Slash:
                return (double)left / (double)right;
            case TokenType.Star:
                return (double)left * (double)right;
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return !IsEqual(left, right);
        }

        //  unrechable.
        return null;
    }

    public bool IsEqual(object left, object right)
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
        var rightExpr = Evaluate(expr.Right);

        switch (expr.Opt.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(rightExpr);
            case TokenType.Minus:
                return -(double)rightExpr;
        }
        
        //  unrechable.
        return null;
    }

    private bool IsTruthy(object rightExpr)
    {
        if (rightExpr is null) return false;
        if (rightExpr is bool b) return b;
        return true;
    }
}