using System.Text;
using LoxGenerator;

namespace Lox;

public class AstPrinter : Expr.IVisitor<String>
{
    public string Print(Expr expr) {
        return expr.Accept(this);
    }

    public string VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string VisitBinaryExpr(Expr.Binary expr)
        => Parenthesize(expr.Opt.Lexeme, expr.Left, expr.Right);

    public string VisitGroupingExpr(Expr.Grouping expr)
        => Parenthesize("group", expr.Expression);

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        if (expr.Value is null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
        => Parenthesize(expr.Opt.Lexeme, expr.Right);

    public string VisitVariableExpr(Expr.Variable expr)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();
        
        builder.Append("(").Append(name);
        foreach (Expr expr in exprs)
        {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");
        
        return builder.ToString();
    }
}