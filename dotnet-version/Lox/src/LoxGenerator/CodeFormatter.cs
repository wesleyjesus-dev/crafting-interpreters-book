using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LoxGenerator;

public static class CodeFormatter
{
    public static string FormatCode(string sourceCode)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot().NormalizeWhitespace();

        return root.ToFullString();
    }
}