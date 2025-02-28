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
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("Token.g.cs", SourceText.From(TokenClasses.Token, Encoding.UTF8));
            
            context.AddSource("TokenType.g.cs", SourceText.From(TokenClasses.TokenEnum, Encoding.UTF8));
            
            var backusNaurFormLox = new Dictionary<string, List<string>>()
            {
                { "Assign"   , ["Token name", "Expr value"] },
                { "Binary"   , ["Expr left", "Token opt", "Expr right"] },
                { "Grouping" , ["Expr expression" ] },
                { "Literal"  , ["Object value" ] },
                { "Unary"    , ["Token opt", "Expr right"] },
                { "Variable", ["Token name"] }
            };
            
            DefineAst("Expr", backusNaurFormLox, context);
            
            var backusNaurFormLoxToStatementAndExpressions = new Dictionary<string, List<string>>()
            {
                { "Block", ["List<Stmt> statements"] },
                { "Expression", ["Expr loxExpression"] },
                { "Print", ["Expr loxExpression"] },
                { "Var", ["Token name", "Expr initializer"] },
            };
            
            DefineAst("Stmt", backusNaurFormLoxToStatementAndExpressions, context);
        }
        private void DefineAst(string baseName, Dictionary<string, List<string>> types, GeneratorExecutionContext context)
        {
            string source = $@"
            using System;
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
            context.AddSource($"{baseName}.g.cs",  CodeFormatter.FormatCode(source));
        }

        private string DefineVisitor(string baseName, Dictionary<string, List<string>> types)
        {
            var methods = String.Join("\n", types.Select(x => $"T Visit{x.Key}{baseName}({x.Key} expr);\n").ToArray());
            return @$"
            public interface IVisitor<T> 
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
                        return visitor.Visit{className}{baseName}(this);
                    }}
                }}";
                
                bodyNestedClasses += "\n";
            }
            return bodyNestedClasses;
        }
    }
}


