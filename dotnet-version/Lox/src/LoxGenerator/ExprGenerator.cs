using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace LoxGenerator
{
    [Generator]
    public class ExprGenerator : ISourceGenerator
    {
        private const string _tokenEnum = @"
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
        private const string _token = @"
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
        
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var backusNaurFormLox = new Dictionary<string, List<string>>()
            {
                { "Binary"   , ["Expr left", "Token opt", "Expr right"] },
                { "Grouping" , ["Expr expression" ] },
                { "Literal"  , ["Object value" ] },
                { "Unary"    , ["Token opt", "Expr right"] },
            };
            
            DefineAst("Expr", backusNaurFormLox, context);
        }
        private void DefineAst(string baseName, Dictionary<string, List<string>> types, GeneratorExecutionContext context)
        {
            string source = $@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoxGenerator
{{
    public abstract class {baseName}
    {{
        {DefineVisitor(baseName, types)}

        public abstract T Accept<T>(IVisitor<T> visitor);

        {DefineType(baseName, types)}
    }}
}}";
            context.AddSource("Token.g.cs", SourceText.From(_token, Encoding.UTF8));
            context.AddSource("TokenType.g.cs", SourceText.From(_tokenEnum, Encoding.UTF8));
            context.AddSource("Expr.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private string DefineVisitor(string baseName, Dictionary<string, List<string>> types)
        {
            var methods = String.Join("\n", types.Select(x => $"T Visit{x.Key}Expr({x.Key} expr);\n").ToArray());
            return @$"public interface IVisitor<T> 
        {{
            {{methods}}
        }}
".Replace("{methods}", methods);
        }

        private string SetUpperCaseOnlyFirstLetter(string input)
        {
            var type = input.Split(' ')[0];
            var variable = input.Split(' ')[1];
            
            return $"{type} {variable[0].ToString().ToUpper() + variable.Substring(1)}";
        }
        private string DefineType(string baseName, Dictionary<string, List<string>> types)
        {
            var bodyNestedClasses = String.Empty;
            
            foreach (var type in types)
            {
                var className = type.Key;
                var propertiesFromClasses = type.Value;

                var props = string.Empty;
                foreach (var property in propertiesFromClasses)
                {
                    var propWithouUpperCase = property.Split(' ')[1];
                    props += $"            this.{propWithouUpperCase[0].ToString().ToUpper() + propWithouUpperCase.Substring(1)} = {propWithouUpperCase};\n";
                }
                    
                var properties = String.Join("\n", type.Value.Select(x => $"            public {SetUpperCaseOnlyFirstLetter(x)} {{ get; set; }}\n"));
                
                bodyNestedClasses += $@"public class {className} : {baseName}
        {{
            {properties}      
    
            public {className}({string.Join(",", propertiesFromClasses)})
            {{
                {props}
            }}

            public override T Accept<T>(IVisitor<T> visitor)
            {{
                return visitor.Visit{className}Expr(this);
            }}
        }}";
                bodyNestedClasses += "\n";
            }

            return bodyNestedClasses;
        }
    }
}


